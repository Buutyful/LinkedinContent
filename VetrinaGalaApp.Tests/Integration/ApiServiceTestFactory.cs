﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Infrastructure;
using VetrinaGalaApp.ApiService.Infrastructure.Models;

namespace VetrinaGalaApp.Tests.Integration;

public sealed class ApiServiceTestFactory : WebApplicationFactory<ProgramApiMarker>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;

    public ApiServiceTestFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove all EF Core related services
            services.RemoveAllDbContext<AppDbContext>();

            // Add DbContext without pooling
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            }, ServiceLifetime.Scoped, ServiceLifetime.Scoped);

            // Reconfigure Identity services
            var identityBuilder = services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
            });

            identityBuilder
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        using var context = CreateDbContext();
        using var roleManager = CreateRoleManager();

        await context.Database.EnsureCreatedAsync();

        await SeedRolesAsync(roleManager);
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public UserManager<User> CreateUserManager()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    }
    public RoleManager<IdentityRole<Guid>> CreateRoleManager()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    }
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roleNames = new RoleConstants().GetConstantRoles();

        foreach (var roleName in roleNames)
        {
            var exists = await roleManager.RoleExistsAsync(roleName);
            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName)
                {
                    NormalizedName = roleManager.NormalizeKey(roleName)
                });
            }
        }        
    }
}