using FluentValidation.TestHelper;
using Users.Api.Models;
using Users.Api.Validators;

namespace Users.Tests.Validators;

[TestFixture]
public class EmploymentValidatorTests
{
    private EmploymentValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new EmploymentValidator();
    }

    [Test]
    public void Should_HaveError_When_Company_IsEmpty()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldHaveValidationErrorFor(e => e.Company)
            .WithErrorMessage("Company is required.");
    }

    [Test]
    public void Should_HaveError_When_Company_IsTooLong()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = new string('A', 151), 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldHaveValidationErrorFor(e => e.Company)
            .WithErrorMessage("Company name cannot exceed 150 characters.");
    }

    [Test]
    public void Should_NotHaveError_When_Company_IsValid()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldNotHaveValidationErrorFor(e => e.Company);
    }

    [Test]
    public void Should_HaveError_When_MonthsOfExperience_IsNull()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = null, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldHaveValidationErrorFor(e => e.MonthsOfExperience)
            .WithErrorMessage("Months of experience is required.");
    }

    [Test]
    public void Should_NotHaveError_When_MonthsOfExperience_IsValid()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldNotHaveValidationErrorFor(e => e.MonthsOfExperience);
    }

    [Test]
    public void Should_HaveError_When_Salary_IsNull()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = null, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldHaveValidationErrorFor(e => e.Salary)
            .WithErrorMessage("Salary is required.");
    }

    [Test]
    public void Should_NotHaveError_When_Salary_IsValid()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldNotHaveValidationErrorFor(e => e.Salary);
    }

    [Test]
    public void Should_HaveError_When_StartDate_IsNull()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = null 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldHaveValidationErrorFor(e => e.StartDate)
            .WithErrorMessage("Start date is required.");
    }

    [Test]
    public void Should_NotHaveError_When_StartDate_IsValid()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15) 
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldNotHaveValidationErrorFor(e => e.StartDate);
    }

    [Test]
    public void Should_HaveError_When_EndDate_IsBeforeStartDate()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15),
            EndDate = new DateTime(2022, 1, 14) // Before start date
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldHaveValidationErrorFor(e => e.EndDate)
            .WithErrorMessage("End date must be after start date.");
    }

    [Test]
    public void Should_NotHaveError_When_EndDate_IsAfterStartDate()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15),
            EndDate = new DateTime(2022, 1, 16) // After start date
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldNotHaveValidationErrorFor(e => e.EndDate);
    }

    [Test]
    public void Should_NotHaveError_When_EndDate_IsNull()
    {
        // Arrange
        var employment = new Employment 
        { 
            Company = "ACME Inc", 
            MonthsOfExperience = 24, 
            Salary = 75000, 
            StartDate = new DateTime(2022, 1, 15),
            EndDate = null // No end date (current job)
        };

        // Act & Assert
        var result = _validator.TestValidate(employment);
        result.ShouldNotHaveValidationErrorFor(e => e.EndDate);
    }
} 