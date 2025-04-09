using System.Diagnostics;


var builder = DistributedApplication.CreateBuilder(args);

//Postgres server
var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin()
                      .WithDataVolume()
                      .WithLifetime(ContainerLifetime.Persistent);
//MinIO container
var minio = builder.AddContainer("minio", "minio/minio")
                   .WithArgs("server", "/data", "--console-address", ":9001")
                   .WithEnvironment("MINIO_ROOT_USER", "minioadmin")
                   .WithEnvironment("MINIO_ROOT_PASSWORD", "minioadmin")
                   .WithBindMount("minio-data", "/data") // Persistent storage
                   .WithHttpEndpoint(targetPort: 9000, name: "http", isProxied: true) // MinIO API
                   .WithHttpEndpoint(targetPort: 9001, name: "console", isProxied: true) // MinIO Console
                   .WithLifetime(ContainerLifetime.Persistent);

//Postgres db
var postgresdb = postgres.AddDatabase("postgresdb");

//Migration Service
var migrator = builder.AddProject<Projects.VetrinaGalaApp_MigrationService>("migrations")
                       .WithReference(postgresdb);

//Api Service
var apiService = builder.AddProject<Projects.VetrinaGalaApp_ApiService>("apiservice")
    .WithReference(postgresdb)
    .WithReference(minio.GetEndpoint("http")) // Reference MinIO API endpoint
    .WithScalar()
    .WaitFor(migrator);

//Web client: https://learn.microsoft.com/en-us/dotnet/aspire/get-started/build-aspire-apps-with-nodejs
builder.AddNpmApp("frontend", "../VetrinaGalaApp.Client")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();

builder.Build().Run();

public static class ResourceBuilderExtentsions
{
    public static IResourceBuilder<T> WithScalar<T>(this IResourceBuilder<T> builder)
        where T : IResourceWithEndpoints
        => builder.WithOpenApiDocs("scalar", "Scalar_Api", "/scalar/v1");
    private static IResourceBuilder<T> WithOpenApiDocs<T>(
        this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string url)
        where T : IResourceWithEndpoints
    {
        builder.WithCommand(name, displayName, async (command) =>
                            {
                                await Task.CompletedTask;
                                try
                                {
                                    var endpoint = builder.GetEndpoint("https");
                                    var Url = $"{endpoint.Url}{url}";
                                    Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true });

                                    return new ExecuteCommandResult { Success = true };
                                }
                                catch (Exception ex)
                                {
                                    return new ExecuteCommandResult { Success = false, ErrorMessage = ex.Message };
                                }
                            });
        return builder;
    }
}
