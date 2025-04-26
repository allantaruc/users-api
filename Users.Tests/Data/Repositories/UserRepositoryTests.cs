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
    
    [Test]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserByIdAsync(1);

        // Assert
        result.ShouldNotBeNull();
        result!.FirstName.ShouldBe("Allan");
        result.LastName.ShouldBe("Taruc");
        result.Email.ShouldBe("allan.b.taruc@gmail.com");
    }
    
    [Test]
    public async Task GetUserByIdAsync_WithNonExistingUser_ShouldThrowException()
    {
        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userRepository.GetUserByIdAsync(0));
    }
    
    [Test]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var existingUser = new User
        {
            FirstName = "Existing",
            LastName = "User",
            Email = "duplicate@example.com"
        };
        
        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();
        
        var newUser = new User
        {
            FirstName = "New",
            LastName = "User",
            Email = "duplicate@example.com"
        };
        
        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userRepository.CreateUserAsync(newUser));
        
        exception.Message.ShouldContain("already exists");
    }
    
    [Test]
    public async Task CreateUserAsync_WithInvalidEmploymentDates_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Employments =
            [
                new Employment
                {
                    Company = "Invalid Dates Corp",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2022, 1, 15),
                    EndDate = new DateTime(2022, 1, 15) // End date same as start date (invalid)
                }
            ]
        };
        
        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userRepository.CreateUserAsync(user));
        
        exception.Message.ShouldContain("end date");
        exception.Message.ShouldContain("must be after start date");
    }
    
    [Test]
    public async Task CreateUserAsync_WithEndDateBeforeStartDate_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Employments =
            [
                new Employment()
                {
                    Company = "Very Bad Corp",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2022, 5, 15),
                    EndDate = new DateTime(2022, 1, 15) // End date before start date (invalid)
                }
            ]
        };
        
        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userRepository.CreateUserAsync(user));
        
        exception.Message.ShouldContain("end date");
        exception.Message.ShouldContain("must be after start date");
    }
    
    [Test]
    public async Task UpdateUserAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var user1 = new User
        {
            FirstName = "First",
            LastName = "User",
            Email = "first@example.com"
        };
        
        var user2 = new User
        {
            FirstName = "Second",
            LastName = "User",
            Email = "second@example.com"
        };
        
        await _dbContext.Users.AddRangeAsync(user1, user2);
        await _dbContext.SaveChangesAsync();
        
        // Try to update user2 with user1's email
        user2.Email = "first@example.com";
        
        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userRepository.UpdateUserAsync(user2));
        
        exception.Message.ShouldContain("already exists");
    }
    
    [Test]
    public async Task UpdateUserAsync_WithInvalidEmploymentDates_ShouldThrowException()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Employments =
            [
                new Employment
                {
                    Company = "Good Corp",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        // Now update with invalid dates
        user.Employments[0].EndDate = new DateTime(2021, 1, 15); // End date before start date
        
        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userRepository.UpdateUserAsync(user));
        
        exception.Message.ShouldContain("end date");
        exception.Message.ShouldContain("must be after start date");
    }
    
    [Test]
    public async Task UpdateUserAsync_ShouldUpdateAddress()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Old Street",
                City = "Old City",
                PostCode = 1000
            }
        };
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        // Retrieve the user for update
        var savedUser = await _dbContext.Users
            .Include(u => u.Address)
            .FirstAsync(u => u.Email == user.Email);
        
        // Modify address
        savedUser.Address!.Street = "New Street";
        savedUser.Address.City = "New City";
        savedUser.Address.PostCode = 2000;
        
        // Act
        var result = await _userRepository.UpdateUserAsync(savedUser);
        
        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("New Street");
        result.Address.City.ShouldBe("New City");
        result.Address.PostCode.ShouldBe(2000);
        
        // Verify the changes were saved to the database
        var updatedUser = await _dbContext.Users
            .Include(u => u.Address)
            .FirstAsync(u => u.Id == result.Id);
        
        updatedUser.Address.ShouldNotBeNull();
        updatedUser.Address!.Street.ShouldBe("New Street");
        updatedUser.Address.City.ShouldBe("New City");
        updatedUser.Address.PostCode.ShouldBe(2000);
    }
    
    [Test]
    public async Task UpdateUserAsync_ShouldUpdateEmployments()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments =
            [
                new Employment
                {
                    Company = "Old Company",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2021, 1, 1)
                }
            ]
        };
        
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        
        // Retrieve the user for update
        var savedUser = await _dbContext.Users
            .Include(u => u.Employments)
            .FirstAsync(u => u.Email == user.Email);
        
        // Modify existing employment and add a new one
        savedUser.Employments[0].Company = "Updated Company";
        savedUser.Employments[0].MonthsOfExperience = 24;
        savedUser.Employments[0].Salary = 75000;
        
        savedUser.Employments.Add(new Employment
        {
            Company = "New Company",
            MonthsOfExperience = 6,
            Salary = 60000,
            StartDate = new DateTime(2022, 1, 1)
        });
        
        // Act
        var result = await _userRepository.UpdateUserAsync(savedUser);
        
        // Assert
        result.ShouldNotBeNull();
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(2);
        
        var updatedEmployment = result.Employments.First(e => e.Company == "Updated Company");
        updatedEmployment.ShouldNotBeNull();
        updatedEmployment.MonthsOfExperience.HasValue.ShouldBeTrue();
        updatedEmployment.MonthsOfExperience!.Value.ShouldBe(24U);
        updatedEmployment.Salary.HasValue.ShouldBeTrue();
        updatedEmployment.Salary!.Value.ShouldBe(75000U);
        
        var newEmployment = result.Employments.First(e => e.Company == "New Company");
        newEmployment.ShouldNotBeNull();
        newEmployment.MonthsOfExperience.HasValue.ShouldBeTrue();
        newEmployment.MonthsOfExperience!.Value.ShouldBe(6U);
        newEmployment.Salary.HasValue.ShouldBeTrue();
        newEmployment.Salary!.Value.ShouldBe(60000U);
        newEmployment.StartDate.ShouldBe(new DateTime(2022, 1, 1));
        newEmployment.EndDate.ShouldBeNull();
        
        // Verify the changes were saved to the database
        var updatedUser = await _dbContext.Users
            .Include(u => u.Employments)
            .FirstAsync(u => u.Id == result.Id);
        
        updatedUser.Employments.Count.ShouldBe(2);
        updatedUser.Employments.Any(e => e.Company == "Updated Company").ShouldBeTrue();
        updatedUser.Employments.Any(e => e.Company == "New Company").ShouldBeTrue();
    }
} 