using Moq;
using Shouldly;
using Users.Api.Data.Repositories;
using Users.Api.Models;
using Users.Api.Services;

namespace Users.Tests.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private IUserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _userService = new UserService(_mockUserRepository.Object);
    }

    [Test]
    public async Task CreateUserAsync_WithValidRequest_ShouldCreateUser()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Paltoc",
                City = "Manila",
                PostCode = 1016
            },
            Employments =
            [
                new Employment
                {
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        var createdUser = new User
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Address = request.Address,
            Employments = request.Employments ?? []
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(1))!
            .ReturnsAsync((User?)null);
        
        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(1);
        result.FirstName.ShouldBe(request.FirstName);
        result.LastName.ShouldBe(request.LastName);
        result.Email.ShouldBe(request.Email);
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Test]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        // Setup mock to throw exception when CreateUserAsync is called
        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException($"A user with email '{request.Email}' already exists."));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("already exists");
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task CreateUserAsync_WithInvalidData_ShouldThrowException()
    {
        // Arrange
        var request = new User
        {
            FirstName = "",  // Invalid: empty first name
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("FirstName");
        
        // The repository method shouldn't be called due to validation failure
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task CreateUserAsync_WithInvalidEmploymentDates_ShouldThrowException()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments =
            [
                new Employment
                {
                    Company = "Problem Corp",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2022, 6, 1),
                    EndDate = new DateTime(2022, 3, 1) // End date before start date (invalid)
                }
            ]
        };

        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new ArgumentException("Employment end date must be after start date."));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("date");
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_WithValidRequest_ShouldUpdateUser()
    {
        // Arrange
        var userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Original",
            LastName = "User",
            Email = "original@example.com",
            Address = new Address
            {
                Street = "Old Street",
                City = "Old City",
                PostCode = 1000
            },
            Employments =
            [
                new Employment
                {
                    Company = "Old Company",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2020, 1, 1)
                }
            ]
        };

        var updateRequest = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Paltoc",
                City = "Manila",
                PostCode = 1016
            },
            Employments =
            [
                new Employment
                {
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
            Address = updateRequest.Address,
            Employments = updateRequest.Employments
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.FirstName.ShouldBe(updateRequest.FirstName);
        result.LastName.ShouldBe(updateRequest.LastName);
        result.Email.ShouldBe(updateRequest.Email);
        
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe(updateRequest.Address!.Street);
        result.Address.City.ShouldBe(updateRequest.Address.City);
        result.Address.PostCode.ShouldBe(updateRequest.Address.PostCode);
        
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(1);
        result.Employments[0].Company.ShouldBe(updateRequest.Employments![0].Company);
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Original",
            LastName = "User",
            Email = "original@example.com"
        };

        var updateRequest = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "duplicate@example.com" // This email is already taken by another user
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new InvalidOperationException($"Another user with email '{updateRequest.Email}' already exists."));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("already exists");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_WithNonExistingUser_ShouldThrowException()
    {
        // Arrange
        const int userId = 999; // Non-existing user ID
        var updateRequest = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))!
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("not found");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_WithInvalidData_ShouldThrowException()
    {
        // Arrange
        var userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Original",
            LastName = "User",
            Email = "original@example.com"
        };

        var updateRequest = new User
        {
            Id = userId,
            FirstName = "", // Invalid: empty first name
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("FirstName");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserAsync_WithInvalidEmploymentDates_ShouldThrowException()
    {
        // Arrange
        var userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Original",
            LastName = "User",
            Email = "original@example.com",
            Employments =
            [
                new Employment
                {
                    Company = "Old Company",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2020, 1, 1)
                }
            ]
        };

        var updateRequest = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments =
            [
                new Employment
                {
                    Company = "Problem Corp",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2022, 6, 1),
                    EndDate = new DateTime(2022, 3, 1) // End date before start date (invalid)
                }
            ]
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new ArgumentException("Employment end date must be after start date."));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("date");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_WithAddressChangesOnly_ShouldUpdateOnlyAddress()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Old Street",
                City = "Old City",
                PostCode = 1000
            },
            Employments =
            [
                new Employment
                {
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        var updateRequest = new User
        {
            Id = userId,
            FirstName = "Allan", // Same name
            LastName = "Taruc", // Same last name
            Email = "allan.b.taruc@gmail.com", // Same email
            Address = new Address
            {
                Street = "New Street",
                City = "New City",
                PostCode = 2000
            }
            // No employment information sent
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = existingUser.FirstName,
            LastName = existingUser.LastName,
            Email = existingUser.Email,
            Address = updateRequest.Address,
            Employments = existingUser.Employments // Should be preserved
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        
        // Address should be updated
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("New Street");
        result.Address.City.ShouldBe("New City");
        result.Address.PostCode.ShouldBe(2000);
        
        // Employments should be preserved
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(1);
        result.Employments[0].Company.ShouldBe("ACME Inc");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task UpdateUserAsync_WithEmploymentChangesOnly_ShouldUpdateOnlyEmployments()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Paltoc",
                City = "Manila",
                PostCode = 1016
            },
            Employments =
            [
                new Employment
                {
                    Company = "Old Company",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2020, 1, 1)
                }
            ]
        };

        var updateRequest = new User
        {
            Id = userId,
            FirstName = "Allan", // Same name
            LastName = "Taruc", // Same last name
            Email = "allan.b.taruc@gmail.com", // Same email
            Employments =
            [
                new Employment
                {
                    Company = "New Company",
                    MonthsOfExperience = 36,
                    Salary = 90000,
                    StartDate = new DateTime(2022, 1, 1)
                },
                new Employment
                {
                    Company = "Second Company",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2020, 1, 1),
                    EndDate = new DateTime(2021, 12, 31)
                }
            ]
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = existingUser.FirstName,
            LastName = existingUser.LastName,
            Email = existingUser.Email,
            Address = existingUser.Address, // Should be preserved
            Employments = updateRequest.Employments
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        
        // Address should be preserved
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("Paltoc");
        result.Address.City.ShouldBe("Manila");
        result.Address.PostCode.ShouldBe(1016);
        
        // Employments should be updated
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(2);
        
        var newEmployment = result.Employments.First(e => e.Company == "New Company");
        newEmployment.ShouldNotBeNull();
        newEmployment.MonthsOfExperience.HasValue.ShouldBeTrue();
        newEmployment.MonthsOfExperience!.Value.ShouldBe(36U);
        newEmployment.Salary.HasValue.ShouldBeTrue();
        newEmployment.Salary!.Value.ShouldBe(90000U);
        
        var secondEmployment = result.Employments.First(e => e.Company == "Second Company");
        secondEmployment.ShouldNotBeNull();
        secondEmployment.MonthsOfExperience.HasValue.ShouldBeTrue();
        secondEmployment.MonthsOfExperience!.Value.ShouldBe(24U);
        secondEmployment.EndDate.HasValue.ShouldBeTrue();
        secondEmployment.EndDate!.Value.ShouldBe(new DateTime(2021, 12, 31));
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Paltoc",
                City = "Manila",
                PostCode = 1016
            },
            Employments =
            [
                new Employment
                {
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(userId);
        result.FirstName.ShouldBe("Allan");
        result.LastName.ShouldBe("Taruc");
        result.Email.ShouldBe("allan.b.taruc@gmail.com");
        
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("Paltoc");
        result.Address.City.ShouldBe("Manila");
        result.Address.PostCode.ShouldBe(1016);
        
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(1);
        result.Employments[0].Company.ShouldBe("ACME Inc");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task GetUserByIdAsync_WithNonExistingUser_ShouldThrowException()
    {
        // Arrange
        const int userId = 999; // Non-existing user ID
        
        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))!
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userService.GetUserByIdAsync(userId));
        
        exception.Message.ShouldContain($"User with ID {userId} not found");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task GetUserByIdAsync_WithRepositoryError_ShouldPropagateException()
    {
        // Arrange
        const int userId = 1;
        
        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ThrowsAsync(new InvalidOperationException("Database connection error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userService.GetUserByIdAsync(userId));
        
        exception.Message.ShouldContain("Database connection error");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
    }
} 