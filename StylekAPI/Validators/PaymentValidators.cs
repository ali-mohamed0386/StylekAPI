using FluentValidation;
using StylekAPI.DTOs.Payments;

namespace StylekAPI.Validators;

public class CreatePaymentIntentDtoValidator : AbstractValidator<CreatePaymentIntentDto>
{
    public CreatePaymentIntentDtoValidator()
    {
        RuleFor(x => x.OrderId).GreaterThan(0);
    }
}
