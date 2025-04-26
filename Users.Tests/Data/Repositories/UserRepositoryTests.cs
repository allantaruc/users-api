using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;
using Users.Api.Data;
using Users.Api.Data.Repositories;
using Users.Api.Models;

namespace Users.Tests.Data.Repositories;

[TestFixture]
public class UserRepositoryTests
{
    private DbContextOptions<AppDbContext> _dbContextOptions = null!;
    private AppDbContext _dbContext = null!;
    private UserRepository _userRepository = null!;

    [SetUp]
    public void Setup()
    {
        // Configure in-memory database
        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserTestDb_{Guid.NewGuid()}")
            .Options;
        
        // Create context and repository
        _dbContext = new AppDbContext(_dbContextOptions);
        _userRepository = new UserRepository(_dbContext);
        
        // Ensure database is created
        _dbContext.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task CreateUserAsync_ShouldCreateUserWithAddress()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Paltoc",
                City = "Manila",
                PostCode = 1016
            }
        };

        // Act
        var result = await _userRepository.CreateUserAsync(user);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);
        
        // Verify the user was saved to the database
        var savedUser = await _dbContext.Users
            .Include(u => u.Address)
            .FirstOrDefaultAsync(u => u.Id == result.Id);
        
        savedUser.ShouldNotBeNull();
        savedUser.FirstName.ShouldBe("Allan");
        savedUser.LastName.ShouldBe("Taruc");
        savedUser.Email.ShouldBe("allan.b.taruc@gmail.com");
        savedUser.Address.ShouldNotBeNull();
        savedUser.Address!.Street.ShouldBe("Paltoc");
        savedUser.Address.City.ShouldBe("Manila");
        savedUser.Address.PostCode.ShouldBe(1016);
    }
    
    [Test]
    public async Task CreateUserAsync_ShouldCreateUserWithEmployments()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments = new List<Employment>
            {
                new Employment
                {
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                },
                new Employment
                {
                    Company = "XYZ Corp",
                    MonthsOfExperience = 12,
                    Salary = 65000,
                    StartDate = new DateTime(2020, 5, 1),
                    EndDate = new DateTime(2021, 5, 1)
                }
            }
        };

        // Act
        var result = await _userRepository.CreateUserAsync(user);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeGreaterThan(0);
        
        // Verify the user was saved to the database
        var savedUser = await _dbContext.Users
            .Include(u => u.Employments)
            .FirstOrDefaultAsync(u => u.Id == result.Id);
        
        savedUser.ShouldNotBeNull();
        savedUser.FirstName.ShouldBe("Allan");
        savedUser.LastName.ShouldBe("Taruc");
        savedUser.Email.ShouldBe("allan.b.taruc@gmail.com");
        
        savedUser.Employments.ShouldNotBeNull();
        savedUser.Employments.Count.ShouldBe(2);
        
        var acmeEmployment = savedUser.Employments.FirstOrDefault(e => e.Company == "ACME Inc");
        acmeEmployment.ShouldNotBeNull();
        acmeEmployment!.MonthsOfExperience.HasValue.ShouldBeTrue();
        acmeEmployment.MonthsOfExperience!.Value.ShouldBe(24U);
        acmeEmployment.Salary.HasValue.ShouldBeTrue();
        acmeEmployment.Salary!.Value.ShouldBe(75000U);
        acmeEmployment.StartDate.ShouldBe(new DateTime(2022, 1, 15));
        acmeEmployment.EndDate.ShouldBeNull();
        
        var xyzEmployment = savedUser.Employments.FirstOrDefault(e => e.Company == "XYZ Corp");
        xyzEmployment.ShouldNotBeNull();
        xyzEmployment!.MonthsOfExperience.HasValue.ShouldBeTrue();
        xyzEmployment.MonthsOfExperience!.Value.ShouldBe(12U);
        xyzEmployment.Salary.HasValue.ShouldBeTrue();
        xyzEmployment.Salary!.Value.ShouldBe(65000U);
        xyzEmployment.StartDate.ShouldBe(new DateTime(2020, 5, 1));
        xyzEmployment.EndDate.ShouldBe(new DateTime(2021, 5, 1));
    }
} 