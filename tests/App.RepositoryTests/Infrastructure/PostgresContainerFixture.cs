using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace App.RepositoryTests.Infrastructure;

public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("app")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public async Task<string> CreateDatabaseAsync()
    {
        if (_container is null)
        {
            throw new InvalidOperationException("Container is not initialized. Ensure Docker is running.");
        }

        var databaseName = $"test_{Guid.NewGuid():N}";
        var adminBuilder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = "postgres"
        };

        await using var admin = new NpgsqlConnection(adminBuilder.ConnectionString);
        await admin.OpenAsync();
        await using (var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin))
        {
            await command.ExecuteNonQueryAsync();
        }

        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }

    public async Task DropDatabaseAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName) || databaseName.Equals("postgres", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        await using var admin = new NpgsqlConnection(adminBuilder.ConnectionString);
        await admin.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{databaseName}\"", admin);
        await command.ExecuteNonQueryAsync();
    }
}
