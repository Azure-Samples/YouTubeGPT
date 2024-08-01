using Microsoft.Extensions.Logging;

namespace YouTubeGPT.Tests.NUnitExtensions;

internal class NUnitLoggingProvider(TextWriter writer, TimeProvider time) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new NUnitLogger(categoryName, writer, time);

    public void Dispose()
    {
    }
}
