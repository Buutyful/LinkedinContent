using VetrinaGalaApp.ApiService.Infrastructure;

namespace VetrinaGalaApp.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected ApiServiceTestFactory Factory { get; }
    protected HttpClient Client { get; private set; } = null!;
    protected AppDbContext DbContext { get; private set; } = null!;

    protected IntegrationTestBase()
    {
        Factory = new ApiServiceTestFactory();
    }

    public async Task InitializeAsync()
    {
        await Factory.InitializeAsync();
        Client = Factory.CreateClient();
        DbContext = Factory.CreateDbContext();
    }

    public async Task DisposeAsync()
    {        
        await Factory.DisposeAsync();
    }

    protected async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }
}