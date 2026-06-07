using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using StylekAPI.Data;
using StylekAPI.DTOs.Payments;
using StylekAPI.Helpers;
using StylekAPI.Models.Enums;

namespace StylekAPI.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly StripeSettings _settings;

    public PaymentService(ApplicationDbContext context, IOptions<StripeSettings> settings)
    {
        _context = context;
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(string userId, CreatePaymentIntentDto dto)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId && o.IsActive)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.PaymentMethod != Models.Enums.PaymentMethod.Stripe)
            throw new InvalidOperationException("Order is not configured for Stripe payment.");

        if (order.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("Order is already paid.");

        var amountInPiastres = (long)(order.TotalAmount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInPiastres,
            Currency = _settings.Currency,
            Metadata = new Dictionary<string, string>
            {
                { "orderId", order.Id.ToString() },
                { "orderNumber", order.OrderNumber }
            }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        order.StripePaymentIntentId = intent.Id;
        await _context.SaveChangesAsync();

        return new PaymentIntentResponseDto
        {
            ClientSecret = intent.ClientSecret,
            PaymentIntentId = intent.Id,
            Amount = order.TotalAmount,
            Currency = _settings.Currency
        };
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret);

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent != null)
                await UpdateOrderPaymentStatusAsync(intent.Id, PaymentStatus.Paid, OrderStatus.Confirmed);
        }
        else if (stripeEvent.Type == "payment_intent.payment_failed")
        {
            var intent = stripeEvent.Data.Object as PaymentIntent;
            if (intent != null)
                await UpdateOrderPaymentStatusAsync(intent.Id, PaymentStatus.Failed, OrderStatus.Pending);
        }
    }

    private async Task UpdateOrderPaymentStatusAsync(string paymentIntentId, PaymentStatus paymentStatus, OrderStatus orderStatus)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntentId);

        if (order == null) return;

        order.PaymentStatus = paymentStatus;
        if (paymentStatus == PaymentStatus.Paid)
            order.Status = orderStatus;

        await _context.SaveChangesAsync();
    }
}
