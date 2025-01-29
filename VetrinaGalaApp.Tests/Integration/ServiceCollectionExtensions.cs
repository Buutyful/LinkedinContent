using Microsoft.EntityFrameworkCore;

namespace VetrinaGalaApp.Tests.Integration;

public static class ServiceCollectionExtensions
{
    public static void RemoveDbContext<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(DbContextOptions<TDbContext>));

        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        var contextDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(TDbContext));

        if (contextDescriptor != null)
        {
            services.Remove(contextDescriptor);
        }
    }
}
