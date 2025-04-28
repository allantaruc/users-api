using FluentValidation.TestHelper;
using Users.Api.Models;
using Users.Api.Validators;

namespace Users.Tests.Validators;

[TestFixture]
public class UserValidatorTests
{
    private UserValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new UserValidator();
    }

    [Test]
    public void Should_HaveError_When_FirstName_IsEmpty()
    {
        // Arrange
        var user = new User { FirstName = "", LastName = "Doe", Email = "john.doe@example.com" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.FirstName)
            .WithErrorMessage("FirstName is required.");
    }

    [Test]
    public void Should_HaveError_When_FirstName_IsTooLong()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = new string('A', 101), 
            LastName = "Doe", 
            Email = "john.doe@example.com" 
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.FirstName)
            .WithErrorMessage("FirstName cannot exceed 100 characters.");
    }

    [Test]
    public void Should_NotHaveError_When_FirstName_IsValid()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(u => u.FirstName);
    }

    [Test]
    public void Should_HaveError_When_LastName_IsEmpty()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "", Email = "john.doe@example.com" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.LastName)
            .WithErrorMessage("LastName is required.");
    }

    [Test]
    public void Should_HaveError_When_LastName_IsTooLong()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = "John", 
            LastName = new string('D', 101), 
            Email = "john.doe@example.com" 
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.LastName)
            .WithErrorMessage("LastName cannot exceed 100 characters.");
    }

    [Test]
    public void Should_NotHaveError_When_LastName_IsValid()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(u => u.LastName);
    }

    [Test]
    public void Should_HaveError_When_Email_IsEmpty()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe", Email = "" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Email is required.");
    }

    [Test]
    public void Should_HaveError_When_Email_IsInvalid()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe", Email = "not-an-email" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Test]
    public void Should_HaveError_When_Email_IsTooLong()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = "John", 
            LastName = "Doe", 
            Email = $"{new string('a', 140)}@example.com" // Over 150 chars
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(u => u.Email)
            .WithErrorMessage("Email cannot exceed 150 characters.");
    }

    [Test]
    public void Should_NotHaveError_When_Email_IsValid()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(u => u.Email);
    }

    [Test]
    public void Should_ValidateAddress_When_AddressIsProvided()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john.doe@example.com",
            Address = new Address { Street = "", City = "New York", PostCode = 10001 }
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor("Address.Street")
            .WithErrorMessage("Street is required.");
    }

    [Test]
    public void Should_ValidateEmployments_When_EmploymentsAreProvided()
    {
        // Arrange
        var user = new User 
        { 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john.doe@example.com",
            Employments = 
            [
                new Employment 
                { 
                    Company = "ACME Inc",
                    // Missing required fields
                }
            ]
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor("Employments[0].MonthsOfExperience")
            .WithErrorMessage("Months of experience is required.");
        result.ShouldHaveValidationErrorFor("Employments[0].Salary")
            .WithErrorMessage("Salary is required.");
        result.ShouldHaveValidationErrorFor("Employments[0].StartDate")
            .WithErrorMessage("Start date is required.");
    }
} 