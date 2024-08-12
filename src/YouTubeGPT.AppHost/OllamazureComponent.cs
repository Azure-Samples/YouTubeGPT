namespace YouTubeGPT.AppHost;
public class OllamazureComponentResource(string name = "ollamazure", string command = "ollamazure") : ExecutableResource(name, command, Environment.CurrentDirectory), IResourceWithConnectionString
{
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create($"Endpoint={this.GetEndpoint("http")};Key=this-will-be-ignored");
}

public static class OllamazureComponentExtensions
{
    public static IResourceBuilder<OllamazureComponentResource> AddOllamazure(this IDistributedApplicationBuilder builder, string name, string chatModelName = "phi3")
    {
        var resource = new OllamazureComponentResource(name);

        var ollama = builder.AddOllama("ollama", modelName: chatModelName);

        return builder.AddResource(resource)
            .WithHttpEndpoint(port: 4041, name: "http", isProxied: false)
            .WithArgs(ctx =>
            {
                ctx.Args.Add("--ollama-url");
                ctx.Args.Add(ollama.Resource.Endpoint.Url);
                ctx.Args.Add("-m");
                ctx.Args.Add(chatModelName);
                ctx.Args.Add("--yes");
            });
    }
}