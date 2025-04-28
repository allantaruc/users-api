using FluentValidation;
using Users.Api.Models;

namespace Users.Api.Validators;

public class EmploymentValidator : AbstractValidator<Employment>
{
    public EmploymentValidator()
    {
        RuleFor(e => e.Company)
            .NotEmpty().WithMessage("Company is required.")
            .MaximumLength(150).WithMessage("Company name cannot exceed 150 characters.");

        RuleFor(e => e.MonthsOfExperience)
            .NotNull().WithMessage("Months of experience is required.");

        RuleFor(e => e.Salary)
            .NotNull().WithMessage("Salary is required.");

        RuleFor(e => e.StartDate)
            .NotNull().WithMessage("Start date is required.");

        // Validate that end date is after start date
        When(e => e.EndDate.HasValue && e.StartDate.HasValue, () =>
        {
            RuleFor(e => e.EndDate)
                .GreaterThan(e => e.StartDate)
                .WithMessage("End date must be after start date.");
        });
    }
} 