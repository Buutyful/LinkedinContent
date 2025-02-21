using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Minio;
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
   
        // Register MinIO client instead of IAmazonS3
        services.AddSingleton<IMinioClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var minioUrl = config["minio:http"];
            if (string.IsNullOrEmpty(minioUrl))
            {
                throw new InvalidOperationException("MinIO endpoint URL is not configured.");
            }

            return new MinioClient()
                .WithEndpoint(minioUrl)
                .WithCredentials("minioadmin", "minioadmin") //TODO: Replace with actual credentials
                .Build();
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