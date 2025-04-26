using Shouldly;
using Users.Api.Models;

namespace Users.Tests.Models;

[TestFixture]
public class ModelTests
{
    [Test]
    public void Address_Properties_ShouldSetAndGet()
    {
        // Arrange
        var address = new Address
        {
            Id = 42,
            Street = "Test Street",
            City = "Test City",
            PostCode = 12345,
            UserId = 123
        };

        // Assert
        address.Id.ShouldBe(42);
        address.Street.ShouldBe("Test Street");
        address.City.ShouldBe("Test City");
        address.PostCode.ShouldBe(12345);
        address.UserId.ShouldBe(123);
    }

    [Test]
    public void Address_WithDefaultValues_ShouldHaveNullablePropertiesAsNull()
    {
        // Arrange
        var address = new Address();

        // Assert
        address.Id.ShouldBe(0);
        address.Street.ShouldBeNull();
        address.City.ShouldBeNull();
        address.PostCode.ShouldBeNull();
        address.UserId.ShouldBe(0);
    }

    [Test]
    public void Employment_Properties_ShouldSetAndGet()
    {
        // Arrange
        var startDate = new DateTime(2022, 1, 1);
        var endDate = new DateTime(2023, 1, 1);
        
        var employment = new Employment
        {
            Id = 42,
            Company = "Test Company",
            MonthsOfExperience = 24,
            Salary = 75000,
            StartDate = startDate,
            EndDate = endDate,
            UserId = 123
        };

        // Assert
        employment.Id.ShouldBe(42);
        employment.Company.ShouldBe("Test Company");
        employment.MonthsOfExperience.HasValue.ShouldBeTrue();
        employment.MonthsOfExperience!.Value.ShouldBe(24U);
        employment.Salary.HasValue.ShouldBeTrue();
        employment.Salary!.Value.ShouldBe(75000U);
        employment.StartDate.ShouldBe(startDate);
        employment.EndDate.ShouldBe(endDate);
        employment.UserId.ShouldBe(123);
    }

    [Test]
    public void Employment_WithDefaultValues_ShouldHaveNullablePropertiesAsNull()
    {
        // Arrange
        var employment = new Employment();

        // Assert
        employment.Id.ShouldBe(0);
        employment.Company.ShouldBeNull();
        employment.MonthsOfExperience.ShouldBeNull();
        employment.Salary.ShouldBeNull();
        employment.StartDate.ShouldBeNull();
        employment.EndDate.ShouldBeNull();
        employment.UserId.ShouldBe(0);
    }

    [Test]
    public void Employment_WithNoEndDate_ShouldHaveEndDateNull()
    {
        // Arrange
        var startDate = new DateTime(2022, 1, 1);
        
        var employment = new Employment
        {
            Company = "Current Company",
            MonthsOfExperience = 12,
            Salary = 65000,
            StartDate = startDate,
            // No end date set
            UserId = 123
        };

        // Assert
        employment.StartDate.ShouldBe(startDate);
        employment.EndDate.ShouldBeNull();
    }
} 