using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Infrastructure.Models;
using VetrinaGalaApp.ApiService.Infrastructure.Security;
using VetrinaGalaApp.ApiService.Infrastructure.Security.Jwt;


namespace VetrinaGalaApp.ApiService.Infrastructure;

public static class InfraDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<AppDbContext>(connectionName: "postgresdb");

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.User.RequireUniqueEmail = true;

        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();       

        services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Section));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services
          .ConfigureOptions<JwtBearerTokenValidationConfiguration>()
          .AddAuthentication(options =>
          {
              options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
              options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
          })
          .AddJwtBearer();

        services.AddScoped<ICurrentUserProvider, UserProvider>();
        return services;
    }
}