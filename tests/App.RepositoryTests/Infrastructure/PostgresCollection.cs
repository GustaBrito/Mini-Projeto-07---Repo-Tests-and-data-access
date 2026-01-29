using Xunit;

namespace App.RepositoryTests.Infrastructure;

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
{
}
