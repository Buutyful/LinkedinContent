using VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;

namespace VetrinaGalaApp.ApiService.Application;

public static class AppDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(IAssemblyMarkerApp).Assembly);

            options.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        return services;
    }
}
public interface IAssemblyMarkerApp;