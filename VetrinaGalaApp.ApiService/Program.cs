using Microsoft.AspNetCore.Authorization;
using VetrinaGalaApp.ApiService;
using VetrinaGalaApp.ApiService.Application;
using VetrinaGalaApp.ApiService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    // Add service defaults & Aspire client integrations.
    builder.AddServiceDefaults();

    // Add services to the container.
    builder.Services
        .AddPresentation()
        .AddInfrastructure(builder)
        .AddApplication();   
}


var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints(); 

    app.Run();
}

