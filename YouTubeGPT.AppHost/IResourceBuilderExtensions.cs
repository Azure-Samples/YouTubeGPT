namespace YouTubeGPT.AppHost;
public static class IResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithConfiguration<T>(this IResourceBuilder<T> builder, string key)
        where T : IResourceWithEnvironment
        => builder.ApplicationBuilder.Configuration[key] is null ?
        builder
        : builder.WithEnvironment(key.Replace(":", "__"), builder.ApplicationBuilder.Configuration[key]);
}
