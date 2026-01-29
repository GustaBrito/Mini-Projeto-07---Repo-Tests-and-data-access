using App.Api.Persistence;
using App.Api.Persistence.Entities;
using App.Api.Persistence.Models;
using App.Api.Persistence.Repositories;
using App.RepositoryTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.RepositoryTests;

[Collection("postgres")]
public sealed class ProductRepositoryEfTests
{
    private readonly PostgresContainerFixture _fixture;

    public ProductRepositoryEfTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [DockerFact]
    public async Task GetByPriceRangeAsync_ReturnsProductsWithinRange()
    {
        var connectionString = await _fixture.CreateDatabaseAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var dbContext = new AppDbContext(options);
            await dbContext.Database.MigrateAsync();

            var electronicsCategory = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
            var groceryCategory = new Category { Id = Guid.NewGuid(), Name = "Grocery" };
            var headphones = new Product { Id = Guid.NewGuid(), Name = "Headphones", Price = 120m, CategoryId = electronicsCategory.Id };
            var keyboard = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Price = 250m, CategoryId = electronicsCategory.Id };
            var coffee = new Product { Id = Guid.NewGuid(), Name = "Coffee", Price = 15m, CategoryId = groceryCategory.Id };
            var monitor = new Product { Id = Guid.NewGuid(), Name = "Monitor", Price = 520m, CategoryId = electronicsCategory.Id };

            dbContext.AddRange(electronicsCategory, groceryCategory, headphones, keyboard, coffee, monitor);
            await dbContext.SaveChangesAsync();

            var repository = new ProductRepositoryEf(dbContext);
            var result = await repository.GetByPriceRangeAsync(50m, 300m, CancellationToken.None);

            result.Should().BeEquivalentTo(
                new[]
                {
                    new ProductInRange(headphones.Id, headphones.Name, headphones.Price, electronicsCategory.Name),
                    new ProductInRange(keyboard.Id, keyboard.Name, keyboard.Price, electronicsCategory.Name)
                },
                options => options.WithStrictOrdering());
        }
        finally
        {
            await _fixture.DropDatabaseAsync(connectionString);
        }
    }

    [DockerFact]
    public async Task GetByPriceRangeAsync_IncludesMinAndMaxBounds()
    {
        var connectionString = await _fixture.CreateDatabaseAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var dbContext = new AppDbContext(options);
            await dbContext.Database.MigrateAsync();

            var electronicsCategory = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
            var cable = new Product { Id = Guid.NewGuid(), Name = "Cable", Price = 50m, CategoryId = electronicsCategory.Id };
            var webcam = new Product { Id = Guid.NewGuid(), Name = "Webcam", Price = 300m, CategoryId = electronicsCategory.Id };

            dbContext.AddRange(electronicsCategory, cable, webcam);
            await dbContext.SaveChangesAsync();

            var repository = new ProductRepositoryEf(dbContext);
            var result = await repository.GetByPriceRangeAsync(50m, 300m, CancellationToken.None);

            result.Should().BeEquivalentTo(
                new[]
                {
                    new ProductInRange(cable.Id, cable.Name, cable.Price, electronicsCategory.Name),
                    new ProductInRange(webcam.Id, webcam.Name, webcam.Price, electronicsCategory.Name)
                },
                options => options.WithStrictOrdering());
        }
        finally
        {
            await _fixture.DropDatabaseAsync(connectionString);
        }
    }
}
