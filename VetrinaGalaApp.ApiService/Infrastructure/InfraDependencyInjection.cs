using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Minio;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Infrastructure.MinIo;
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

        services
            .AddMinioClient(builder.Configuration)
            .AddSecurity(builder.Configuration);

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

        return services;
    }
    private static IServiceCollection AddMinioClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioSettings>(configuration.GetSection("MinioSettings"));

        // Register MinIO client
        services.AddSingleton<IMinioClient>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<MinioSettings>>().Value;
            var endpoint = Environment.GetEnvironmentVariable("services__minio__http") ?? "http://localhost:9000";

            return new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.UseSSL)
                .Build();
        });

        services.AddScoped<IMinioService, MinioService>();

        return services;
    }

    private static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));
        services.Configure<GoogleSettings>(configuration.GetSection(GoogleSettings.Section));

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services
          .ConfigureOptions<JwtBearerTokenValidationConfiguration>()
          .AddAuthentication(options =>
          {
              options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
              options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
          })
          .AddJwtBearer()
          .AddCookie(IdentityConstants.ExternalScheme)
          .AddGoogle(options =>
          {
              var googleSettings = configuration.GetSection("Google").Get<GoogleSettings>();

              if (googleSettings == null || string.IsNullOrEmpty(googleSettings.ClientId) || string.IsNullOrEmpty(googleSettings.ClientSecret))
              {
                  throw new InvalidOperationException("Google ClientId and ClientSecret must be configured.");
              }

              options.ClientId = googleSettings.ClientId;
              options.ClientSecret = googleSettings.ClientSecret;
              // IMPORTANT: Tell Google handler to use the External Cookie scheme
              // This allows SignInManager.GetExternalLoginInfoAsync() to work correctly
              options.SignInScheme = IdentityConstants.ExternalScheme;
          });

        services.AddScoped<ICurrentUserProvider, UserProvider>();

        return services;
    }
}

//MINIO IS S3 API COMPATIBLE, COULD USE AWS SDK DIRECTLTY
//builder.Services.AddSingleton<IAmazonS3>(sp =>
//        {
//            var config = sp.GetRequiredService<IConfiguration>();
//var minioUrl = config["minio:http"];
//            return new AmazonS3Client(
//                "minioadmin", // MINIO_ROOT_USER
//                "minioadmin", // MINIO_ROOT_PASSWORD
//                new AmazonS3Config
//                {
//                    ServiceURL = minioUrl,
//                    ForcePathStyle = true // Required for MinIO
//                });
//        });