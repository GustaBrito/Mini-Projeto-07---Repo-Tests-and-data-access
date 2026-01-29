using App.Api.Persistence.Models;

namespace App.Api.Persistence.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<ProductInRange>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken);
}
