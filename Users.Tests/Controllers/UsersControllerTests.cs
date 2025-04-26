using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Users.Api.Controllers;
using Users.Api.Models;
using Users.Api.Services;

namespace Users.Tests.Controllers;

[TestFixture]
public class UsersControllerTests
{
    private Mock<IUserService> _mockUserService = null!;
    private Mock<ILogger<UsersController>> _mockLogger = null!;
    private UsersController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(_mockUserService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task CreateUser_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        var createdUser = new User
        {
            Id = 1,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };

        _mockUserService.Setup(service => service.CreateUserAsync(request))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.ShouldNotBeNull();
        var createdAtResult = result.Result.ShouldBeOfType<CreatedAtActionResult>();
        createdAtResult.ActionName.ShouldBe(nameof(UsersController.GetUserById));
        if (createdAtResult.RouteValues != null)
        {
            createdAtResult.RouteValues.ShouldContainKey("id");
            createdAtResult.RouteValues["id"].ShouldBe(1);
        }

        var returnValue = createdAtResult.Value.ShouldBeOfType<User>();
        returnValue.Id.ShouldBe(1);
        returnValue.FirstName.ShouldBe("Allan");
        returnValue.LastName.ShouldBe("Taruc");
        returnValue.Email.ShouldBe("allan.b.taruc@gmail.com");
        
        _mockUserService.Verify(service => service.CreateUserAsync(request), Times.Once);
    }

    [Test]
    public async Task CreateUser_WithValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new User
        {
            FirstName = "", // Invalid: empty first name
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserService.Setup(service => service.CreateUserAsync(request))
            .ThrowsAsync(new ArgumentException("FirstName is required."));

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.ShouldNotBeNull();
        var badRequestResult = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldBe("FirstName is required.");
        
        _mockUserService.Verify(service => service.CreateUserAsync(request), Times.Once);
    }

    [Test]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com" // Email that already exists
        };

        _mockUserService.Setup(service => service.CreateUserAsync(request))
            .ThrowsAsync(new InvalidOperationException("A user with email 'allan.b.taruc@gmail.com' already exists."));

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.ShouldNotBeNull();
        var conflictResult = result.Result.ShouldBeOfType<ConflictObjectResult>();
        conflictResult.Value.ShouldBe("A user with email 'allan.b.taruc@gmail.com' already exists.");
        
        _mockUserService.Verify(service => service.CreateUserAsync(request), Times.Once);
    }

    [Test]
    public async Task CreateUser_WithUnexpectedException_ShouldReturnStatusCode500()
    {
        // Arrange
        var request = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserService.Setup(service => service.CreateUserAsync(request))
            .ThrowsAsync(new Exception("Unexpected database error"));

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.ShouldNotBeNull();
        var statusCodeResult = result.Result.ShouldBeOfType<ObjectResult>();
        statusCodeResult.StatusCode.ShouldBe(500);
        statusCodeResult.Value.ShouldBe("An error occurred while creating the user.");
        
        _mockUserService.Verify(service => service.CreateUserAsync(request), Times.Once);
    }

    [Test]
    public async Task GetUserById_WithExistingUser_ShouldReturnOk()
    {
        // Arrange
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        result.ShouldNotBeNull();
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<User>();
        returnValue.Id.ShouldBe(userId);
        returnValue.FirstName.ShouldBe("Allan");
        returnValue.LastName.ShouldBe("Taruc");
        returnValue.Email.ShouldBe("allan.b.taruc@gmail.com");
        
        _mockUserService.Verify(service => service.GetUserByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetUserById_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Arrange
        const int userId = 999;

        _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
            .ThrowsAsync(new InvalidOperationException($"User with ID {userId} not found."));

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        result.ShouldNotBeNull();
        var notFoundResult = result.Result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldBe($"User with ID {userId} not found.");
        
        _mockUserService.Verify(service => service.GetUserByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetUserById_WithException_ShouldBePropagated()
    {
        // Arrange
        const int userId = 1;

        _mockUserService.Setup(service => service.GetUserByIdAsync(userId))
            .ThrowsAsync(new Exception("Database connection error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(
            async () => await _controller.GetUserById(userId));
        
        exception.Message.ShouldBe("Database connection error");
        
        _mockUserService.Verify(service => service.GetUserByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithValidData_ShouldReturnOk()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "New Street",
                City = "New City",
                PostCode = 2000
            }
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
            Address = updateRequest.Address
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<User>();
        returnValue.Id.ShouldBe(userId);
        returnValue.FirstName.ShouldBe("Allan");
        returnValue.LastName.ShouldBe("Taruc");
        returnValue.Email.ShouldBe("allan.b.taruc@gmail.com");
        returnValue.Address.ShouldNotBeNull();
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithValidationError_ShouldReturnBadRequest()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new User
        {
            FirstName = "", // Invalid: empty first name
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ThrowsAsync(new ArgumentException("FirstName is required."));

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var badRequestResult = result.Result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.Value.ShouldBe("FirstName is required.");
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithNonExistingUser_ShouldReturnNotFound()
    {
        // Arrange
        const int userId = 999; // Non-existing user ID
        var updateRequest = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ThrowsAsync(new InvalidOperationException($"User with ID {userId} not found."));

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var notFoundResult = result.Result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldBe($"User with ID {userId} not found.");
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "duplicate@example.com" // Already used by another user
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ThrowsAsync(new InvalidOperationException("Another user with email 'duplicate@example.com' already exists."));

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var notFoundResult = result.Result.ShouldBeOfType<NotFoundObjectResult>();
        notFoundResult.Value.ShouldBe("Another user with email 'duplicate@example.com' already exists.");
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithUnexpectedException_ShouldReturnStatusCode500()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new User
        {
            FirstName = "Allan",
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com"
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ThrowsAsync(new Exception("Unexpected database error"));

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var statusCodeResult = result.Result.ShouldBeOfType<ObjectResult>();
        statusCodeResult.StatusCode.ShouldBe(500);
        statusCodeResult.Value.ShouldBe("An error occurred while updating the user.");
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithAddressChangesOnly_ShouldUpdateAddress()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new User
        {
            FirstName = "Allan", // Required fields
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Address = new Address
            {
                Street = "New Street",
                City = "New City",
                PostCode = 2000
            }
            // No employment information
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
            Address = updateRequest.Address,
            Employments = new List<Employment>
            {
                new Employment
                {
                    Company = "Existing Company", // This should remain unchanged
                    MonthsOfExperience = 24,
                    Salary = 75000,
                    StartDate = new DateTime(2022, 1, 15)
                }
            }
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<User>();
        
        // Check updated address
        returnValue.Address.ShouldNotBeNull();
        returnValue.Address!.Street.ShouldBe("New Street");
        returnValue.Address.City.ShouldBe("New City");
        returnValue.Address.PostCode.ShouldBe(2000);
        
        // Check employments preserved
        returnValue.Employments.ShouldNotBeNull();
        returnValue.Employments.Count.ShouldBe(1);
        returnValue.Employments[0].Company.ShouldBe("Existing Company");
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }

    [Test]
    public async Task UpdateUser_WithEmploymentChangesOnly_ShouldUpdateEmployments()
    {
        // Arrange
        const int userId = 1;
        var updateRequest = new User
        {
            FirstName = "Allan", // Required fields
            LastName = "Taruc",
            Email = "allan.b.taruc@gmail.com",
            Employments = new List<Employment>
            {
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
            }
        };

        var updatedUser = new User
        {
            Id = userId,
            FirstName = updateRequest.FirstName,
            LastName = updateRequest.LastName,
            Email = updateRequest.Email,
            Address = new Address
            {
                Street = "Existing Street", // This should remain unchanged
                City = "Existing City",
                PostCode = 1000
            },
            Employments = updateRequest.Employments
        };

        _mockUserService.Setup(service => service.UpdateUserAsync(userId, updateRequest))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _controller.UpdateUser(userId, updateRequest);

        // Assert
        result.ShouldNotBeNull();
        var okResult = result.Result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<User>();
        
        // Check address preserved
        returnValue.Address.ShouldNotBeNull();
        returnValue.Address!.Street.ShouldBe("Existing Street");
        returnValue.Address.City.ShouldBe("Existing City");
        returnValue.Address.PostCode.ShouldBe(1000);
        
        // Check updated employments
        returnValue.Employments.ShouldNotBeNull();
        returnValue.Employments.Count.ShouldBe(2);
        
        var newEmployment = returnValue.Employments.First(e => e.Company == "New Company");
        newEmployment.ShouldNotBeNull();
        newEmployment.MonthsOfExperience.HasValue.ShouldBeTrue();
        newEmployment.MonthsOfExperience!.Value.ShouldBe(36U);
        newEmployment.Salary.HasValue.ShouldBeTrue();
        newEmployment.Salary!.Value.ShouldBe(90000U);
        
        var secondEmployment = returnValue.Employments.First(e => e.Company == "Second Company");
        secondEmployment.ShouldNotBeNull();
        secondEmployment.MonthsOfExperience.HasValue.ShouldBeTrue();
        secondEmployment.MonthsOfExperience!.Value.ShouldBe(24U);
        secondEmployment.EndDate.HasValue.ShouldBeTrue();
        secondEmployment.EndDate!.Value.ShouldBe(new DateTime(2021, 12, 31));
        
        _mockUserService.Verify(service => service.UpdateUserAsync(userId, updateRequest), Times.Once);
    }
} 