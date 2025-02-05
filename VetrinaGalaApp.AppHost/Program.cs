using System.Diagnostics;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
                      .WithPgAdmin()
                      .WithDataVolume()
                      .WithLifetime(ContainerLifetime.Persistent);

var postgresdb = postgres.AddDatabase("postgresdb");

var migrator = builder.AddProject<Projects.VetrinaGalaApp_MigrationService>("migrations")
                       .WithReference(postgresdb);

var apiService = builder.AddProject<Projects.VetrinaGalaApp_ApiService>("apiservice")
    .WithReference(postgresdb)
    .WithScalar()
    .WaitFor(migrator);

builder.AddProject<Projects.VetrinaGalaApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);


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