using Moq;
using NUnit.Framework;
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
} 