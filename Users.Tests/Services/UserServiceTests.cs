using FluentValidation;
using FluentValidation.Results;
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
    private Mock<IValidator<User>> _mockValidator = null!;
    private IUserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockValidator = new Mock<IValidator<User>>();
        
        // Setup default validation to pass for all requests
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
            
        _userService = new UserService(_mockUserRepository.Object, _mockValidator.Object);
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

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("FirstName", "FirstName is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("FirstName");
        
        // The repository method shouldn't be called due to validation failure
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
            FirstName = "", // Invalid: empty first name
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };
        
        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("FirstName", "FirstName is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

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

    [Test]
    public async Task CreateUserAsync_WithNullEmployments_ShouldSetEmptyList()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments = []
        };

        var createdUser = new User
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Employments = []
        };

        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(0);
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.Is<User>(u => 
            u.Employments.Count == 0)), Times.Once);
    }
    
    [Test]
    public async Task CreateUserAsync_WithMissingAddressStreet_ShouldThrowException()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                // Missing Street
                City = "Manila",
                PostCode = 1016
            }
        };

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Address.Street", "Street is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("Street");
    }
    
    [Test]
    public async Task CreateUserAsync_WithMissingAddressCity_ShouldThrowException()
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
                // Missing City
                PostCode = 1016
            }
        };

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Address.City", "City is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("City");
    }
    
    [Test]
    public async Task CreateUserAsync_WithMissingEmploymentCompany_ShouldThrowException()
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
                    // Missing Company
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Employments[0].Company", "Company is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("Company");
    }
    
    [Test]
    public async Task CreateUserAsync_WithMissingEmploymentMonthsOfExperience_ShouldThrowException()
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
                    Company = "ACME Inc",
                    // Missing MonthsOfExperience
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Employments[0].MonthsOfExperience", "Months of experience is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("experience");
    }
    
    [Test]
    public async Task CreateUserAsync_WithMissingEmploymentSalary_ShouldThrowException()
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
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    // Missing Salary
                    StartDate = new DateTime(2022, 1, 15)
                }
            ]
        };

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Employments[0].Salary", "Salary is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("Salary");
    }
    
    [Test]
    public async Task CreateUserAsync_WithMissingEmploymentStartDate_ShouldThrowException()
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
                    Company = "ACME Inc",
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    // Missing StartDate
                }
            ]
        };

        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Employments[0].StartDate", "Start date is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("Start date");
    }
    
    [Test]
    public async Task UpdateUserAsync_WithNoEmployments_ShouldPreserveExistingEmployments()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
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
            FirstName = "New Allan",
            LastName = "New Taruc",
            Email = "new.allan@gmail.com",
            Employments = [] // Empty list - should not update existing employments
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
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
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(1);
        result.Employments[0].Company.ShouldBe("ACME Inc");
        
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u => 
            u.Employments == existingUser.Employments)), Times.Once);
    }

    [Test]
    public async Task GetAllUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                Id = 1,
                FirstName = "Allan",
                LastName = "Taruc",
                Email = "allan@example.com"
            },
            new()
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com"
            }
        };

        _mockUserRepository.Setup(repo => repo.GetAllUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        var usersList = result.ToList();
        usersList.ShouldNotBeNull();
        usersList.Count.ShouldBe(2);
        usersList.ShouldContain(u => u.Email == "allan@example.com");
        usersList.ShouldContain(u => u.Email == "jane@example.com");
        
        _mockUserRepository.Verify(repo => repo.GetAllUsersAsync(), Times.Once);
    }
    
    [Test]
    public async Task GetAllUsersAsync_WhenNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        _mockUserRepository.Setup(repo => repo.GetAllUsersAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        var usersList = result.ToList();
        usersList.ShouldNotBeNull();
        usersList.Count.ShouldBe(0);
        
        _mockUserRepository.Verify(repo => repo.GetAllUsersAsync(), Times.Once);
    }
    
    [Test]
    public async Task GetAllUsersAsync_WithRepositoryError_ShouldPropagateException()
    {
        // Arrange
        _mockUserRepository.Setup(repo => repo.GetAllUsersAsync())
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            async () => await _userService.GetAllUsersAsync());
        
        exception.Message.ShouldContain("Database connection error");
        
        _mockUserRepository.Verify(repo => repo.GetAllUsersAsync(), Times.Once);
    }
    
    [Test]
    public async Task DeleteUserAsync_WithExistingUser_ShouldDeleteUser()
    {
        // Arrange
        const int userId = 1;
        
        _mockUserRepository.Setup(repo => repo.DeleteUserAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        await _userService.DeleteUserAsync(userId);

        // Assert
        _mockUserRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task DeleteUserAsync_WithNonExistingUser_ShouldPropagateException()
    {
        // Arrange
        const int userId = 999;
        
        _mockUserRepository.Setup(repo => repo.DeleteUserAsync(userId))
            .ThrowsAsync(new InvalidOperationException($"User with ID {userId} not found."));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _userService.DeleteUserAsync(userId));
        
        exception.Message.ShouldContain("not found");
        
        _mockUserRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task DeleteUserAsync_WithRepositoryError_ShouldPropagateException()
    {
        // Arrange
        const int userId = 1;
        
        _mockUserRepository.Setup(repo => repo.DeleteUserAsync(userId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            async () => await _userService.DeleteUserAsync(userId));
        
        exception.Message.ShouldContain("Database connection error");
        
        _mockUserRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
    }

    [Test]
    public async Task CreateUserAsync_WithEmptyEmploymentsList_ShouldCreateUserWithEmptyList()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments = []
        };

        var createdUser = new User
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Employments = []
        };

        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(0);
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.Is<User>(u => 
            u.Employments.Count == 0)), Times.Once);
    }
    
    [Test]
    public async Task CreateUserAsync_WithNullAddress_ShouldCreateUserWithoutAddress()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = null
        };

        var createdUser = new User
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Address = null
        };

        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldBeNull();
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.Is<User>(u => 
            u.Address == null)), Times.Once);
    }
    
    [Test]
    public async Task UpdateUserAsync_WithNoAddress_ShouldPreserveExistingAddress()
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
                Street = "Existing Street",
                City = "Existing City",
                PostCode = 1000
            }
        };

        var updateRequest = new User
        {
            FirstName = "New Allan",
            LastName = "New Taruc",
            Email = "new.allan@gmail.com",
            Address = null // No address in update request
        };

        var expectedUpdatedUser = new User
        {
            Id = userId,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
            Address = existingUser.Address // Should be preserved
        };

        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUpdatedUser);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("Existing Street");
        result.Address.City.ShouldBe("Existing City");
        
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u => 
            u.Address == existingUser.Address)), Times.Once);
    }
    
    [Test]
    public async Task UpdateUserAsync_WithNullEmail_ShouldThrowException()
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
            FirstName = "Allan",
            LastName = "Taruc",
            Email = null // Invalid: null email
        };
        
        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Email is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("Email");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }
    
    [Test]
    public async Task UpdateUserAsync_WithNullFirstName_ShouldThrowException()
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
            FirstName = null, // Invalid: null first name
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };
        
        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("FirstName", "FirstName is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("FirstName");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }
    
    [Test]
    public async Task UpdateUserAsync_WithNullLastName_ShouldThrowException()
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
            FirstName = "Allan",
            LastName = null, // Invalid: null last name
            Email = "allan.b.taruc@gmail.com"
        };
        
        _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync(existingUser);
        
        // Setup mock validator to fail validation
        var validationFailures = new List<ValidationFailure>
        {
            new ValidationFailure("LastName", "LastName is required.")
        };
        
        _mockValidator
            .Setup(v => v.ValidateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.UpdateUserAsync(userId, updateRequest));
        
        exception.Message.ShouldContain("LastName");
        
        _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task CreateUserAsync_WithAddressMissingPostCode_ShouldStillCreateUser()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "Test Street",
                City = "Test City",
                PostCode = null // PostCode is optional
            }
        };

        var createdUser = new User
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Address = request.Address
        };

        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("Test Street");
        result.Address.City.ShouldBe("Test City");
        result.Address.PostCode.ShouldBeNull();
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.Is<User>(u => 
            u.Address!.PostCode == null)), Times.Once);
    }
    
    [Test]
    public async Task CreateUserAsync_WithBothAddressAndEmployments_ShouldCreateComplete()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Full",
            LastName = "User",
            Email = "full.user@example.com",
            Address = new Address
            {
                Street = "Complete Street",
                City = "Complete City",
                PostCode = 1234
            },
            Employments =
            [
                new Employment
                {
                    Company = "Complete Company",
                    MonthsOfExperience = 36,
                    Salary = 80000,
                    StartDate = new DateTime(2020, 1, 1)
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
            Employments = request.Employments
        };

        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.ShouldNotBeNull();
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("Complete Street");
        result.Address.City.ShouldBe("Complete City");
        result.Address.PostCode.ShouldBe(1234);
        
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(1);
        result.Employments[0].Company.ShouldBe("Complete Company");
        result.Employments[0].MonthsOfExperience.HasValue.ShouldBeTrue();
        result.Employments[0].MonthsOfExperience!.Value.ShouldBe(36U);
        result.Employments[0].Salary.HasValue.ShouldBeTrue();
        result.Employments[0].Salary!.Value.ShouldBe(80000U);
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.Is<User>(u => 
            u.Address!.City == "Complete City" && 
            u.Employments.Count == 1)), Times.Once);
    }
    
    [Test]
    public async Task UpdateUserAsync_WithBothAddressAndEmploymentChanges_ShouldUpdateBoth()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            FirstName = "Original",
            LastName = "User",
            Email = "original@example.com",
            Address = new Address
            {
                Street = "Original Street",
                City = "Original City",
                PostCode = 1000
            },
            Employments =
            [
                new Employment
                {
                    Company = "Original Company",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2020, 1, 1)
                }
            ]
        };

        var updateRequest = new User
        {
            FirstName = "Updated",
            LastName = "User",
            Email = "updated@example.com",
            Address = new Address
            {
                Street = "New Street",
                City = "New City",
                PostCode = 2000
            },
            Employments =
            [
                new Employment
                {
                    Company = "New Company",
                    MonthsOfExperience = 24,
                    Salary = 70000,
                    StartDate = new DateTime(2021, 1, 1)
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
        
        // Basic properties
        result.FirstName.ShouldBe("Updated");
        result.LastName.ShouldBe("User");
        result.Email.ShouldBe("updated@example.com");
        
        // Address should be updated
        result.Address.ShouldNotBeNull();
        result.Address!.Street.ShouldBe("New Street");
        result.Address.City.ShouldBe("New City");
        result.Address.PostCode.ShouldBe(2000);
        
        // Employments should be updated
        result.Employments.ShouldNotBeNull();
        result.Employments.Count.ShouldBe(1);
        result.Employments[0].Company.ShouldBe("New Company");
        result.Employments[0].MonthsOfExperience.HasValue.ShouldBeTrue();
        result.Employments[0].MonthsOfExperience!.Value.ShouldBe(24U);
        result.Employments[0].Salary.HasValue.ShouldBeTrue();
        result.Employments[0].Salary!.Value.ShouldBe(70000U);
        
        _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u => 
            u.Address!.City == "New City" && 
            u.Employments[0].Company == "New Company")), Times.Once);
    }
    
    [Test]
    public async Task CreateUserAsync_WithInvalidEmploymentEndDate_RepositoryThrows()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Employments =
            [
                new Employment
                {
                    Company = "Test Company",
                    MonthsOfExperience = 12,
                    Salary = 50000,
                    StartDate = new DateTime(2022, 1, 1),
                    EndDate = new DateTime(2021, 1, 1) // End date before start date
                }
            ]
        };

        // Repository will validate employment dates
        _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new ArgumentException("Employment end date (01/01/2021) must be after start date (01/01/2022) for company 'Test Company'."));

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await _userService.CreateUserAsync(request));
        
        exception.Message.ShouldContain("Employment end date");
        exception.Message.ShouldContain("must be after start date");
        
        _mockUserRepository.Verify(repo => repo.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }
} 