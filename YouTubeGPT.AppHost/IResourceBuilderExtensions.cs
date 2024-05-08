namespace YouTubeGPT.AppHost;
public static class IResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithConfiguration<T>(this IResourceBuilder<T> builder, string key)
        where T : IResourceWithEnvironment
        => builder.WithEnvironment(key.Replace(":", "__"), builder.ApplicationBuilder.Configuration[key]);
}
