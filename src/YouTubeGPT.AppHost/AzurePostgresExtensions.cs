namespace YouTubeGPT.AppHost;
internal static class AzurePostgresExtensions
{
    public static IResourceBuilder<PostgresServerResource> AsAzurePostgresFlexibleServerWithVectorSupport(this IResourceBuilder<PostgresServerResource> resource)
    {
        var template = resource.ApplicationBuilder.AddBicepTemplateString("vector-extension", """
    param postgresServerName string

    @description('')
    param location string = resourceGroup().location

    resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' existing = {
        name: postgresServerName
    }

    resource postgresConfig 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2022-12-01' = {
      parent: postgresServer
      name: 'azure.extensions'
      properties: {
        value: 'VECTOR'
        source: 'user-override'
      }
    }
    """);

#pragma warning disable AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        resource.AsAzurePostgresFlexibleServer((resource, construct, server) =>
        {
            construct.AddOutput(server.AddOutput("name", data => data.Name));
            template.WithParameter("postgresServerName", resource.GetOutput("name"));
        });
#pragma warning restore AZPROVISION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        return resource;
    }
}
