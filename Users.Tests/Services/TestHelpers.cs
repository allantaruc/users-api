using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Users.Api.Data;
using Users.Api.Data.Repositories;
using Users.Api.Models;
using Users.Api.Models.Auth;
using Users.Api.Services;

namespace Users.Tests.Services;

public static class TestHelpers
{
    public static AppDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    private static JwtSettings CreateJwtSettings()
    {
        return new JwtSettings
        {
            SecretKey = "TestSecretKeyWithAtLeast32Characters123456",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60
        };
    }

    public static AuthService CreateAuthService(
        AppDbContext dbContext,
        IUserRepository? userRepository = null,
        JwtSettings? jwtSettings = null)
    {
        userRepository ??= CreateUserRepository(dbContext);
        jwtSettings ??= CreateJwtSettings();

        var jwtSettingsOptions = Options.Create(jwtSettings);
        var logger = new Mock<ILogger<AuthService>>();

        return new AuthService(
            userRepository,
            jwtSettingsOptions,
            logger.Object,
            dbContext);
    }

    private static IUserRepository CreateUserRepository(AppDbContext dbContext)
    {
        var mock = new Mock<IUserRepository>();
        
        mock.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                dbContext.Users.Add(user);
                dbContext.SaveChanges();
                return user;
            });

        mock.Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))!
            .ReturnsAsync((int id) => dbContext.Users.Find(id));

        mock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                dbContext.Users.Update(user);
                dbContext.SaveChanges();
                return user;
            });

        return mock.Object;
    }
} 