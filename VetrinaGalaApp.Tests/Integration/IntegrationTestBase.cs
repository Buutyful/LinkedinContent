using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.Tests.Integration;

public class IntegrationTestBase : IAsyncLifetime
{
    private IServiceScope _scope = null!;
    public ApiServiceTestFactory Factory { get; }
    public HttpClient Client { get; private set; } = null!;
    public AppDbContext DbContext { get; private set; } = null!;

    public IntegrationTestBase()
    {
        Factory = new ApiServiceTestFactory();
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();
        _scope = Factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task DisposeAsync()
    {
        _scope.Dispose();
        await Factory.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await Factory.InitializeAsync();
    }
}