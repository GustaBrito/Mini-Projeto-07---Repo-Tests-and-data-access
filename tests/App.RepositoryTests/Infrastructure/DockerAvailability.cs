namespace App.RepositoryTests.Infrastructure;

internal static class DockerAvailability
{
    private const string DockerPipe = @"\\.\pipe\docker_engine";

    public static bool IsAvailable()
    {
        if (OperatingSystem.IsWindows())
        {
            return File.Exists(DockerPipe);
        }

        var dockerHost = Environment.GetEnvironmentVariable("DOCKER_HOST");
        if (!string.IsNullOrWhiteSpace(dockerHost))
        {
            return true;
        }

        return File.Exists("/var/run/docker.sock");
    }
}
