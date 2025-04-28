using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Users.Api.Controllers;
using Users.Api.Models.Auth;
using Users.Api.Services;

namespace Users.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService = null!;
    private Mock<ILogger<AuthController>> _mockLogger = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task Register_WhenSuccessful_ShouldReturnOkWithAuthResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };

        var authResponse = new AuthResponse
        {
            Token = "jwt-token",
            Expiration = 1631234567,
            User = new UserInfo
            {
                Id = 1,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            }
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<AuthResponse>();
        returnValue.Token.ShouldBe("jwt-token");
        returnValue.User!.Email.ShouldBe(request.Email);
    }

    [Test]
    public async Task Register_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "",
            LastName = "",
            Email = "",
            Password = "",
            ConfirmPassword = ""
        };
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Register_WhenServiceReturnsNull_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123",
            ConfirmPassword = "P@ssw0rd123"
        };

        _mockAuthService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync((AuthResponse)null!);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Login_WhenSuccessful_ShouldReturnOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "john.doe@example.com",
            Password = "P@ssw0rd123"
        };

        var authResponse = new AuthResponse
        {
            Token = "jwt-token",
            Expiration = 1631234567,
            User = new UserInfo
            {
                Id = 1,
                Email = request.Email,
                FirstName = "John",
                LastName = "Doe"
            }
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnValue = okResult.Value.ShouldBeOfType<AuthResponse>();
        returnValue.Token.ShouldBe("jwt-token");
        returnValue.User!.Email.ShouldBe(request.Email);
    }

    [Test]
    public async Task Login_WhenModelStateInvalid_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = ""
        };
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Login_WhenServiceReturnsNull_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "john.doe@example.com",
            Password = "WrongPassword"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync((AuthResponse)null!);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public void ValidateToken_WhenTokenIsValid_ShouldReturnOk()
    {
        // Arrange
        const string token = "valid-jwt-token";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = $"Bearer {token}";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _mockAuthService.Setup(s => s.ValidateToken(token))
            .Returns(true);

        // Act
        var result = _controller.ValidateToken();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        // We can't directly cast to a concrete type since it's an anonymous type
        // Check that the status code is 200 OK
        okResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        // Convert to string and check it contains "Valid":true
        (okResult.Value?.ToString()!).ShouldContain("Valid = True");
    }

    [Test]
    public void ValidateToken_WhenNoAuthHeader_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = _controller.ValidateToken();

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public void ValidateToken_WhenAuthHeaderNotBearer_ShouldReturnUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Basic dXNlcjpwYXNzd29yZA==";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = _controller.ValidateToken();

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public void ValidateToken_WhenTokenIsInvalid_ShouldReturnUnauthorized()
    {
        // Arrange
        const string token = "invalid-jwt-token";
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        _mockAuthService.Setup(s => s.ValidateToken(token))
            .Returns(false);

        // Act
        var result = _controller.ValidateToken();

        // Assert
        result.ShouldBeOfType<UnauthorizedObjectResult>();
    }
} 