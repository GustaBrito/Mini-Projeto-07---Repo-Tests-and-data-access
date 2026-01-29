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
public sealed class ProductRepositoryDapperTests
{
    private readonly PostgresContainerFixture _fixture;

    public ProductRepositoryDapperTests(PostgresContainerFixture fixture)
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

            var electronics = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
            var grocery = new Category { Id = Guid.NewGuid(), Name = "Grocery" };
            var productA = new Product { Id = Guid.NewGuid(), Name = "Headphones", Price = 120m, CategoryId = electronics.Id };
            var productB = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Price = 250m, CategoryId = electronics.Id };
            var productC = new Product { Id = Guid.NewGuid(), Name = "Coffee", Price = 15m, CategoryId = grocery.Id };
            var productD = new Product { Id = Guid.NewGuid(), Name = "Monitor", Price = 520m, CategoryId = electronics.Id };

            dbContext.AddRange(electronics, grocery, productA, productB, productC, productD);
            await dbContext.SaveChangesAsync();

            var repository = new ProductRepositoryDapper(connectionString);
            var result = await repository.GetByPriceRangeAsync(50m, 300m, CancellationToken.None);

            result.Should().BeEquivalentTo(
                new[]
                {
                    new ProductInRange(productA.Id, productA.Name, productA.Price, electronics.Name),
                    new ProductInRange(productB.Id, productB.Name, productB.Price, electronics.Name)
                },
                options => options.WithStrictOrdering());
        }
        finally
        {
            await _fixture.DropDatabaseAsync(connectionString);
        }
    }

    [DockerFact]
    public async Task GetByPriceRangeAsync_ReturnsEmptyWhenNoMatches()
    {
        var connectionString = await _fixture.CreateDatabaseAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var dbContext = new AppDbContext(options);
            await dbContext.Database.MigrateAsync();

            var electronics = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Keyboard", Price = 250m, CategoryId = electronics.Id };

            dbContext.AddRange(electronics, product);
            await dbContext.SaveChangesAsync();

            var repository = new ProductRepositoryDapper(connectionString);
            var result = await repository.GetByPriceRangeAsync(1m, 20m, CancellationToken.None);

            result.Should().BeEmpty();
        }
        finally
        {
            await _fixture.DropDatabaseAsync(connectionString);
        }
    }

    [DockerFact]
    public async Task GetByPriceRangeAsync_ReturnsResultsOrderedByPriceThenName()
    {
        var connectionString = await _fixture.CreateDatabaseAsync();
        try
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            await using var dbContext = new AppDbContext(options);
            await dbContext.Database.MigrateAsync();

            var electronics = new Category { Id = Guid.NewGuid(), Name = "Electronics" };
            var productA = new Product { Id = Guid.NewGuid(), Name = "Adapter", Price = 120m, CategoryId = electronics.Id };
            var productB = new Product { Id = Guid.NewGuid(), Name = "Cable", Price = 120m, CategoryId = electronics.Id };
            var productC = new Product { Id = Guid.NewGuid(), Name = "Mouse", Price = 80m, CategoryId = electronics.Id };

            dbContext.AddRange(electronics, productA, productB, productC);
            await dbContext.SaveChangesAsync();

            var repository = new ProductRepositoryDapper(connectionString);
            var result = await repository.GetByPriceRangeAsync(50m, 200m, CancellationToken.None);

            result.Should().BeEquivalentTo(
                new[]
                {
                    new ProductInRange(productC.Id, productC.Name, productC.Price, electronics.Name),
                    new ProductInRange(productA.Id, productA.Name, productA.Price, electronics.Name),
                    new ProductInRange(productB.Id, productB.Name, productB.Price, electronics.Name)
                },
                options => options.WithStrictOrdering());
        }
        finally
        {
            await _fixture.DropDatabaseAsync(connectionString);
        }
    }
}
