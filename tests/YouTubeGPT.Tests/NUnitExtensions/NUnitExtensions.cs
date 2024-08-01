using Microsoft.Extensions.Logging;

namespace YouTubeGPT.Tests.NUnitExtensions;
internal static class NUnitExtensions
{
    public static void AddNUnit(this ILoggingBuilder builder) => 
        builder.Services.AddSingleton<ILoggerProvider>(static _ => new NUnitLoggingProvider(TestContext.Out, TimeProvider.System));
}
