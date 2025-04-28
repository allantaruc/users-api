using FluentValidation;
using Users.Api.Models;

namespace Users.Api.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(u => u.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(100).WithMessage("FirstName cannot exceed 100 characters.");

        RuleFor(u => u.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(100).WithMessage("LastName cannot exceed 100 characters.");

        RuleFor(u => u.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(150).WithMessage("Email cannot exceed 150 characters.");
            // Email uniqueness is checked at the repository level

        // Address validation using a nested validator
        When(u => u.Address != null, () =>
        {
            RuleFor(u => u.Address!)
                .SetValidator(new AddressValidator());
        });

        // Employments validation
        RuleForEach(u => u.Employments).SetValidator(new EmploymentValidator());
    }
} 