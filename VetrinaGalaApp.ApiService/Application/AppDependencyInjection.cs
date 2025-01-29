using VetrinaGalaApp.ApiService.Application.MiddlewareBehaviors;
using FluentValidation;

namespace VetrinaGalaApp.ApiService.Application;

public static class AppDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(AppDependencyInjection).Assembly);
            options.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            options.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddValidatorsFromAssemblyContaining(typeof(AppDependencyInjection));
        return services;
    }
}
