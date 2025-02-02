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
    .WaitFor(migrator);

builder.AddProject<Projects.VetrinaGalaApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);


builder.Build().Run();


