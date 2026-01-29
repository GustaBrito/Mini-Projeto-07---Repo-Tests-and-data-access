using Xunit;

namespace App.RepositoryTests.Infrastructure;

public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerAvailability.IsAvailable())
        {
            Skip = "Docker is not available. Integration tests require a running Docker engine.";
        }
    }
}
