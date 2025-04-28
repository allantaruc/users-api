using FluentValidation;
using Users.Api.Models;

namespace Users.Api.Validators;

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(a => a.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.");

        RuleFor(a => a.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(a => a.PostCode)
            .GreaterThan(0).When(a => a.PostCode.HasValue).WithMessage("Post code must be greater than 0.");
    }
} 