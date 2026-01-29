using App.Api.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Api.Persistence.Repositories;

public sealed class ProductRepositoryEf : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepositoryEf(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductInRange>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Price >= minPrice && product.Price <= maxPrice)
            .OrderBy(product => product.Price)
            .ThenBy(product => product.Name)
            .Select(product => new ProductInRange(
                product.Id,
                product.Name,
                product.Price,
                product.Category.Name))
            .ToListAsync(cancellationToken);
    }
}
