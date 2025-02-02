using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.MigrationService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.AddServiceDefaults();
        builder.Services.AddHostedService<Worker>();
        builder.AddNpgsqlDbContext<AppDbContext>("postgresdb");
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

        var host = builder.Build();
        host.Run();
    }
}