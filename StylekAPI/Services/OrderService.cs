using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StylekAPI.Data;
using StylekAPI.DTOs.Admin;
using StylekAPI.DTOs.Orders;
using StylekAPI.Helpers;
using StylekAPI.Models;
using StylekAPI.Models.Enums;

namespace StylekAPI.Services;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly CartService _cartService;
    private readonly EmailService _emailService;
    private readonly IMapper _mapper;
    private const decimal DefaultShippingFee = 50m;

    public OrderService(
        ApplicationDbContext context,
        CartService cartService,
        EmailService emailService,
        IMapper mapper)
    {
        _context = context;
        _cartService = cartService;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<OrderDetailDto> CreateOrderAsync(string userId, CreateOrderDto dto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Include(c => c.ProductVariant)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty.");

            foreach (var item in cartItems)
            {
                var stock = item.ProductVariant?.Stock ?? item.Product.Stock;
                if (item.Quantity > stock)
                    throw new InvalidOperationException($"Insufficient stock for {item.Product.NameEn}.");
            }

            var subTotal = cartItems.Sum(i => (i.Product.DiscountPrice ?? i.Product.Price) * i.Quantity);
            Coupon? coupon = null;
            decimal discountAmount = 0;

            var couponCode = dto.CouponCode ?? _cartService.GetAppliedCouponCode(userId);
            if (!string.IsNullOrEmpty(couponCode))
            {
                coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive);

                if (coupon != null && coupon.ExpiryDate >= DateTime.UtcNow &&
                    coupon.UsedCount < coupon.MaxUses && subTotal >= coupon.MinOrderAmount)
                {
                    discountAmount = coupon.DiscountPercent.HasValue
                        ? Math.Round(subTotal * coupon.DiscountPercent.Value / 100, 2)
                        : Math.Min(coupon.DiscountAmount ?? 0, subTotal);
                }
            }

            var shippingFee = DefaultShippingFee;
            var total = subTotal - discountAmount + shippingFee;

            var order = new Order
            {
                OrderNumber = OrderNumberGenerator.Generate(),
                UserId = userId,
                Status = OrderStatus.Pending,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = dto.PaymentMethod == PaymentMethod.CashOnDelivery
                    ? PaymentStatus.Pending
                    : PaymentStatus.Pending,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingFee = shippingFee,
                TotalAmount = total,
                ShippingFullName = dto.ShippingFullName,
                ShippingPhone = dto.ShippingPhone,
                ShippingAddress = dto.ShippingAddress,
                ShippingCity = dto.ShippingCity,
                CouponId = coupon?.Id
            };

            foreach (var item in cartItems)
            {
                var unitPrice = item.Product.DiscountPrice ?? item.Product.Price;
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.Product.NameEn,
                    Size = item.ProductVariant?.Size,
                    Color = item.ProductVariant?.Color,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                });

                if (item.ProductVariant != null)
                    item.ProductVariant.Stock -= item.Quantity;
                else
                    item.Product.Stock -= item.Quantity;
            }

            if (coupon != null && discountAmount > 0)
                coupon.UsedCount++;

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _cartService.ClearAppliedCoupon(userId);

            var user = await _context.Users.FindAsync(userId);
            if (user?.Email != null)
                await _emailService.SendOrderConfirmationAsync(user.Email, order.OrderNumber, order.TotalAmount);

            return await GetOrderDetailAsync(userId, order.Id);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderDto>> GetMyOrdersAsync(string userId)
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId && o.IsActive)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDetailDto> GetOrderDetailAsync(string userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            ?? throw new KeyNotFoundException("Order not found.");

        return _mapper.Map<OrderDetailDto>(order);
    }

    public async Task CancelOrderAsync(string userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be cancelled.");

        foreach (var item in order.OrderItems)
        {
            if (item.ProductVariantId.HasValue)
            {
                var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId.Value);
                if (variant != null) variant.Stock += item.Quantity;
            }
            else
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null) product.Stock += item.Quantity;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.IsActive = false;
        await _context.SaveChangesAsync();
    }

    public async Task<TrackOrderDto> TrackOrderAsync(string userId, int orderId)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId && o.IsActive)
            ?? throw new KeyNotFoundException("Order not found.");

        var steps = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Processing,
            OrderStatus.Shipped,
            OrderStatus.Delivered
        };

        var currentIndex = Array.IndexOf(steps, order.Status);
        if (order.Status == OrderStatus.Cancelled)
        {
            return new TrackOrderDto
            {
                OrderNumber = order.OrderNumber,
                CurrentStatus = order.Status,
                Steps = new List<TrackStepDto>
                {
                    new() { Status = OrderStatus.Cancelled, Label = "Cancelled", IsCompleted = true, IsCurrent = true }
                }
            };
        }

        var trackSteps = steps.Select((status, index) => new TrackStepDto
        {
            Status = status,
            Label = status.ToString(),
            IsCompleted = index <= currentIndex,
            IsCurrent = index == currentIndex
        }).ToList();

        return new TrackOrderDto
        {
            OrderNumber = order.OrderNumber,
            CurrentStatus = order.Status,
            Steps = trackSteps
        };
    }

    // --- Admin ---

    public async Task<PagedResult<AdminOrderDto>> GetAllAdminAsync(AdminListFilterDto filter)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .AsQueryable();

        if (!filter.IncludeInactive)
            query = query.Where(o => o.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = $"%{filter.Search.Trim()}%";
            query = query.Where(o => EF.Functions.Like(o.OrderNumber, term));
        }

        var totalCount = await query.CountAsync();
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<AdminOrderDto>
        {
            Items = orders.Select(o => new AdminOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.User.FullName,
                CustomerEmail = o.User.Email ?? string.Empty,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                IsActive = o.IsActive,
                CreatedAt = o.CreatedAt
            }).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderDetailDto> GetAdminOrderDetailAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Coupon)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        return _mapper.Map<OrderDetailDto>(order);
    }

    public async Task<OrderDetailDto> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        order.Status = dto.Status;
        await _context.SaveChangesAsync();
        return await GetAdminOrderDetailAsync(orderId);
    }

    public async Task SoftDeleteAsync(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        order.IsActive = false;
        if (order.Status != OrderStatus.Cancelled)
            order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    public async Task HardDeleteAsync(int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");

        _context.OrderItems.RemoveRange(order.OrderItems);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
    }
}
