using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Users.Api.Data;

namespace Users.Tests.Data;

[TestFixture]
public class DbInitializerTests
{
    [Test]
    public void Initialize_ShouldEnsureDatabaseIsCreated()
    {
        // Arrange - Create a real service provider with in-memory database
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => 
            options.UseInMemoryDatabase("TestDbInitializer"));
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Act - This should create the database
        DbInitializer.Initialize(serviceProvider);
        
        // Assert - Verify the database was created by checking if we can use it
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // If the database wasn't created, this would throw an exception
        Assert.That(dbContext.Database.IsInMemory(), Is.True);
        
        // Additional verification - we can add and query data
        dbContext.Users.Add(new Api.Models.User 
        { 
            FirstName = "Test", 
            LastName = "User", 
            Email = "test@example.com" 
        });
        dbContext.SaveChanges();
        
        Assert.That(dbContext.Users.Count(), Is.EqualTo(1));
    }
} 