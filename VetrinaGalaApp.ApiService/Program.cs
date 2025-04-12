using Scalar.AspNetCore;
using VetrinaGalaApp.ApiService;
using VetrinaGalaApp.ApiService.Application;
using VetrinaGalaApp.ApiService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Services.AddCors(o => o.AddPolicy("all", policy =>
    {
        //scalar client opend from aspire dashboard
        policy.WithOrigins("https://localhost:7493");
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
        policy.AllowCredentials();
    }));

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
    app.UseCors("all");
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapDefaultEndpoints(); 
    app.MapEndPoints();

    app.Run();
}
public class ProgramApiMarker { }