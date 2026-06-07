using AutoMapper;
using StylekAPI.DTOs.Auth;
using StylekAPI.DTOs.Cart;
using StylekAPI.DTOs.Categories;
using StylekAPI.DTOs.Home;
using StylekAPI.DTOs.Orders;
using StylekAPI.DTOs.Products;
using StylekAPI.DTOs.Profile;
using StylekAPI.DTOs.Reviews;
using StylekAPI.Models;

namespace StylekAPI.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>();
        CreateMap<ApplicationUser, ProfileDto>();

        CreateMap<Category, CategoryDto>();

        CreateMap<Banner, BannerDto>();

        CreateMap<Product, ProductListDto>()
            .ForMember(d => d.PrimaryImageUrl, o => o.MapFrom(s => s.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault(i => i.IsPrimary) != null
                ? s.Images.First(i => i.IsPrimary).ImageUrl
                : s.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault()))
            .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Where(r => r.IsActive).Any()
                ? s.Reviews.Where(r => r.IsActive).Average(r => r.Rating) : 0))
            .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Reviews.Count(r => r.IsActive)))
            .ForMember(d => d.CategoryNameEn, o => o.MapFrom(s => s.Category.NameEn));

        CreateMap<Product, ProductDetailDto>()
            .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Reviews.Where(r => r.IsActive).Any()
                ? s.Reviews.Where(r => r.IsActive).Average(r => r.Rating) : 0))
            .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Reviews.Count(r => r.IsActive)));

        CreateMap<Category, CategorySummaryDto>();
        CreateMap<ProductImage, ProductImageDto>();
        CreateMap<ProductVariant, ProductVariantDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.OrderItems.Count));

        CreateMap<Order, OrderDetailDto>()
            .ForMember(d => d.CouponCode, o => o.MapFrom(s => s.Coupon != null ? s.Coupon.Code : null));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.UnitPrice * s.Quantity));

        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.UserAvatarUrl, o => o.MapFrom(s => s.User.AvatarUrl))
            .ForMember(d => d.IsLikedByCurrentUser, o => o.Ignore());
    }
}
