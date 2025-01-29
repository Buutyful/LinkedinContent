using Microsoft.EntityFrameworkCore;
using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.Tests.Integration;

public static class ServiceCollectionExtensions
{
    public static void RemoveAllDbContext<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        var descriptors = services
               .Where(d => d.ServiceType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore") == true)
               .ToList();

        foreach (var descriptor in descriptors)
        {
            services.Remove(descriptor);
        }

        // Remove existing DbContext registration
        var dbContextDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(AppDbContext));
        if (dbContextDescriptor != null)
        {
            services.Remove(dbContextDescriptor);
        }

    }
}
