using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected ApiServiceTestFactory Factory { get; }
    protected HttpClient Client { get; private set; } = null!;
    private IServiceScope _scope = null!;
    protected AppDbContext DbContext { get; private set; } = null!;

    protected IntegrationTestBase()
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

    protected async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }
}