using Microsoft.EntityFrameworkCore;
using Users.Api.Models;

namespace Users.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Employment> Employments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Address)
            .WithOne()
            .HasForeignKey<Address>("UserId");

        modelBuilder.Entity<User>()
            .HasMany(u => u.Employments)
            .WithOne()
            .HasForeignKey("UserId");

        // Configure required fields
        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .IsRequired();
            
        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .IsRequired();
            
        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired();

        modelBuilder.Entity<Address>()
            .Property(a => a.Street)
            .IsRequired();
            
        modelBuilder.Entity<Address>()
            .Property(a => a.City)
            .IsRequired();

        modelBuilder.Entity<Employment>()
            .Property(e => e.Company)
            .IsRequired();
            
        modelBuilder.Entity<Employment>()
            .Property(e => e.MonthsOfExperience)
            .IsRequired();
            
        modelBuilder.Entity<Employment>()
            .Property(e => e.Salary)
            .IsRequired();
            
        modelBuilder.Entity<Employment>()
            .Property(e => e.StartDate)
            .IsRequired();
    }
} 