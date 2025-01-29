using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Testcontainers.PostgreSql;
using VetrinaGalaApp.ApiService.Domain;
using VetrinaGalaApp.ApiService.Infrastructure;
using VetrinaGalaApp.ApiService.Infrastructure.Security;

namespace VetrinaGalaApp.Tests.Integration;

public sealed class ApiServiceTestFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private AppDbContext _dbContext = null!;

    public ApiServiceTestFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove existing database context and identity services
            services.RemoveDbContext<AppDbContext>();
            services.RemoveAll<UserManager<User>>();
            services.RemoveAll<SignInManager<User>>();

            // Add test database context
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_dbContainer.GetConnectionString()));

            // Add Identity configuration matching production
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

            // Configure JWT settings for testing
            services.Configure<JwtSettings>(options =>
            {
                options.Secret = "test-secret-at-least-256-bits-long-1234567890";
                options.TokenExpirationInMinutes = 60;
                options.Issuer = "test-issuer";
                options.Audience = "test-audience";
            });

            // Add authentication scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        _dbContext = CreateDbContext();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;

        return new AppDbContext(options);
    }

    public UserManager<User> CreateUserManager()
    {
        var serviceProvider = Services.CreateScope().ServiceProvider;
        return serviceProvider.GetRequiredService<UserManager<User>>();
    }
}