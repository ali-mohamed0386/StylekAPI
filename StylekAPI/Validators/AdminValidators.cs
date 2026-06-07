using FluentValidation;
using StylekAPI.DTOs.Admin;

namespace StylekAPI.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Gender).IsInEnum();
    }
}

public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductDtoValidator() => Include(new CreateProductDtoValidator());
}

public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
{
    public CreateCategoryDtoValidator()
    {
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Gender).IsInEnum();
    }
}

public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator() => Include(new CreateCategoryDtoValidator());
}

public class CreateBannerDtoValidator : AbstractValidator<CreateBannerDto>
{
    public CreateBannerDtoValidator()
    {
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TitleAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ImageUrl).NotEmpty();
    }
}

public class UpdateBannerDtoValidator : AbstractValidator<UpdateBannerDto>
{
    public UpdateBannerDtoValidator() => Include(new CreateBannerDtoValidator());
}

public class CreateCouponDtoValidator : AbstractValidator<CreateCouponDto>
{
    public CreateCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxUses).GreaterThan(0);
        RuleFor(x => x).Must(x => x.DiscountPercent.HasValue || x.DiscountAmount.HasValue)
            .WithMessage("Either DiscountPercent or DiscountAmount is required.");
    }
}

public class UpdateCouponDtoValidator : AbstractValidator<UpdateCouponDto>
{
    public UpdateCouponDtoValidator() => Include(new CreateCouponDtoValidator());
}

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator() => RuleFor(x => x.Status).IsInEnum();
}

public class UpdateUserRolesDtoValidator : AbstractValidator<UpdateUserRolesDto>
{
    public UpdateUserRolesDtoValidator()
    {
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).Must(r => r is "Admin" or "Manager" or "Customer");
    }
}
