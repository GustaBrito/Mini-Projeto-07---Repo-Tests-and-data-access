using App.Api.Persistence;
using App.Api.Persistence.Entities;
using App.Api.Persistence.Repositories;
using App.RepositoryTests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace App.RepositoryTests;

[Collection("postgres")]
public sealed class ProductRepositoryConsistencyTests
{
    private readonly PostgresContainerFixture _fixture;

    public ProductRepositoryConsistencyTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [DockerFact]
    public async Task EfCoreAndDapper_ReturnSameResults()
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

            dbContext.AddRange(electronics, grocery, productA, productB, productC);
            await dbContext.SaveChangesAsync();

            var efRepository = new ProductRepositoryEf(dbContext);
            var dapperRepository = new ProductRepositoryDapper(connectionString);

            var efResult = await efRepository.GetByPriceRangeAsync(0m, 300m, CancellationToken.None);
            var dapperResult = await dapperRepository.GetByPriceRangeAsync(0m, 300m, CancellationToken.None);

            efResult.Should().BeEquivalentTo(dapperResult, options => options.WithStrictOrdering());
        }
        finally
        {
            await _fixture.DropDatabaseAsync(connectionString);
        }
    }
}
