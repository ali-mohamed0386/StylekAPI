using FluentValidation;
using StylekAPI.DTOs.Products;

namespace StylekAPI.Validators;

public class ProductFilterDtoValidator : AbstractValidator<ProductFilterDto>
{
    public ProductFilterDtoValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice.");
    }
}
