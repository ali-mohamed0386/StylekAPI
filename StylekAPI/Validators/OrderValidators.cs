using FluentValidation;
using StylekAPI.DTOs.Orders;

namespace StylekAPI.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.ShippingFullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ShippingPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(300);
        RuleFor(x => x.ShippingCity).NotEmpty().MaximumLength(100);
    }
}
