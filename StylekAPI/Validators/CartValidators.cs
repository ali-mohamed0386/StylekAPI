using FluentValidation;
using StylekAPI.DTOs.Cart;

namespace StylekAPI.Validators;

public class AddCartItemDtoValidator : AbstractValidator<AddCartItemDto>
{
    public AddCartItemDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class UpdateCartQuantityDtoValidator : AbstractValidator<UpdateCartQuantityDto>
{
    public UpdateCartQuantityDtoValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public class ApplyCouponDtoValidator : AbstractValidator<ApplyCouponDto>
{
    public ApplyCouponDtoValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
    }
}
