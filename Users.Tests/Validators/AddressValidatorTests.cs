using FluentValidation.TestHelper;
using Users.Api.Models;
using Users.Api.Validators;

namespace Users.Tests.Validators;

[TestFixture]
public class AddressValidatorTests
{
    private AddressValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new AddressValidator();
    }

    [Test]
    public void Should_HaveError_When_Street_IsEmpty()
    {
        // Arrange
        var address = new Address { Street = "", City = "New York", PostCode = 10001 };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.Street)
            .WithErrorMessage("Street is required.");
    }

    [Test]
    public void Should_HaveError_When_Street_IsTooLong()
    {
        // Arrange
        var address = new Address 
        { 
            Street = new string('A', 201), 
            City = "New York", 
            PostCode = 10001 
        };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.Street)
            .WithErrorMessage("Street cannot exceed 200 characters.");
    }

    [Test]
    public void Should_NotHaveError_When_Street_IsValid()
    {
        // Arrange
        var address = new Address { Street = "123 Main St", City = "New York", PostCode = 10001 };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldNotHaveValidationErrorFor(a => a.Street);
    }

    [Test]
    public void Should_HaveError_When_City_IsEmpty()
    {
        // Arrange
        var address = new Address { Street = "123 Main St", City = "", PostCode = 10001 };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.City)
            .WithErrorMessage("City is required.");
    }

    [Test]
    public void Should_HaveError_When_City_IsTooLong()
    {
        // Arrange
        var address = new Address 
        { 
            Street = "123 Main St", 
            City = new string('A', 101), 
            PostCode = 10001 
        };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.City)
            .WithErrorMessage("City cannot exceed 100 characters.");
    }

    [Test]
    public void Should_NotHaveError_When_City_IsValid()
    {
        // Arrange
        var address = new Address { Street = "123 Main St", City = "New York", PostCode = 10001 };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldNotHaveValidationErrorFor(a => a.City);
    }

    [Test]
    public void Should_HaveError_When_PostCode_IsLessThanOrEqualToZero()
    {
        // Arrange
        var address = new Address { Street = "123 Main St", City = "New York", PostCode = 0 };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldHaveValidationErrorFor(a => a.PostCode)
            .WithErrorMessage("Post code must be greater than 0.");
    }

    [Test]
    public void Should_NotHaveError_When_PostCode_IsValid()
    {
        // Arrange
        var address = new Address { Street = "123 Main St", City = "New York", PostCode = 10001 };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldNotHaveValidationErrorFor(a => a.PostCode);
    }

    [Test]
    public void Should_NotHaveError_When_PostCode_IsNull()
    {
        // Arrange
        var address = new Address { Street = "123 Main St", City = "New York", PostCode = null };

        // Act & Assert
        var result = _validator.TestValidate(address);
        result.ShouldNotHaveValidationErrorFor(a => a.PostCode);
    }
} 