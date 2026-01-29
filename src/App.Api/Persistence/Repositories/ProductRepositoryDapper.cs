using App.Api.Persistence.Models;
using Dapper;
using Npgsql;

namespace App.Api.Persistence.Repositories;

public sealed class ProductRepositoryDapper : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepositoryDapper(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<ProductInRange>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken)
    {
        const string sql = """
select p.id,
       p.name,
       p.price,
       c.name as categoryname
  from products p
  join categories c on c.id = p.category_id
 where p.price between @MinPrice and @MaxPrice
 order by p.price, p.name;
""";

        await using var connection = new NpgsqlConnection(_connectionString);
        var command = new CommandDefinition(
            sql,
            new { MinPrice = minPrice, MaxPrice = maxPrice },
            cancellationToken: cancellationToken);

        var results = await connection.QueryAsync<ProductInRange>(command);
        return results.AsList();
    }
}
