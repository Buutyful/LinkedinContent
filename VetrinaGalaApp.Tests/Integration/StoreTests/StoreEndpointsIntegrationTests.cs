using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Security.Claims;
using VetrinaGalaApp.ApiService.Application.Common.Security;
using VetrinaGalaApp.ApiService.Domain;
using VetrinaGalaApp.ApiService.EndPoints;
using VetrinaGalaApp.ApiService.Infrastructure.Security.Jwt;
using VetrinaGalaApp.Tests.Integration.AuthTests;

namespace VetrinaGalaApp.Tests.Integration.StoreTests;

public class StoreOwnerConversionTests(IntegrationTestBase integrationTestBase)
    : IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _base = integrationTestBase;

    [Fact]
    public async Task CreateStore_ForValidUser_ConvertsToStoreOwner()
    {
        // Arrange       
        var token = await UserTestHelpers.RegisterTestUserAsync(_base.Client, GenerateRandomName(), GenerateRandomEmail());
        using var client = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, token);


        // Act
        var response = await client.PostAsJsonAsync("store/register",
            new CreateStoreRequest("Test Store", "Test Description"));

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

        // Verify user properties
        var userManager = _base.Factory.CreateUserManager();
        var user = await userManager.FindByEmailAsync("test@example.com");

        Assert.NotNull(user);
        Assert.Equal(UserType.StoreOwner, user.UserType);       

        // Verify store creation
        var store = await _base.DbContext.Stores.FirstAsync();
        Assert.Equal("Test Store", store.Name);
        Assert.Equal(user.Id, store.UserId);

        // Verify role assignment
        var roles = await userManager.GetRolesAsync(user);
        Assert.Contains(RoleConstants.StoreOwner, roles);

        // Verify token regeneration
        Assert.NotNull(result?.Token);
        Assert.NotEqual(token, result.Token); // Should get new token with updated claims
    }

    [Fact]
    public async Task CreateStore_ForNonExistentUser_ReturnsNotFound()
    {
        // Arrange        
        using var client = _base.Factory.CreateClient();

        // Act - Try with random user ID
        var response = await client.PostAsJsonAsync("store/register",
            new CreateStoreRequest("Test Store", "Test Description"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateStore_ForExistingStoreOwner_ReturnsBadRequest()
    {
        // Arrange        
        var token = await UserTestHelpers.RegisterTestUserAsync(_base.Client);
        using var client = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, token);

        var response = await client.PostAsJsonAsync("store/register", new CreateStoreRequest("Test Store", "Test Description"));
        response.EnsureSuccessStatusCode();

        var newToken = response.Content.ReadFromJsonAsync<AuthenticationResult>();
        Assert.NotNull(newToken);
        using var secondClient = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, newToken.Result.Token);
        // Act
        var res = await secondClient.PostAsJsonAsync("store/register",
            new CreateStoreRequest("Second Store", "Should Fail"));        

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }



    [Fact]
    public async Task CreateStore_WithValidRequest_UpdatesJwtClaims()
    {
        // Arrange        
        var token = await UserTestHelpers.RegisterTestUserAsync(_base.Client, GenerateRandomName(), GenerateRandomEmail());
        using var client = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, token);

        // Act
        var response = await client.PostAsJsonAsync("store/register",
            new CreateStoreRequest("Test Store", "Test Description"));
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();
        using var newClient = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, result.Token);

        // Verify new token has correct claims
        var checkResponse = await newClient.GetAsync("/auth/check");
        checkResponse.EnsureSuccessStatusCode();

        var claims = await newClient.GetFromJsonAsync<ClaimDto[]>("/auth/claims");
        
        Assert.Contains(claims, c => c.Type == JtwClaimTypesConstants.OwnedStoreId && !string.IsNullOrEmpty(c.Value));
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == RoleConstants.StoreOwner);
    }
    

    private static string GenerateRandomName() =>
        new string(Guid.NewGuid().ToString().Take(6).ToArray());
    private static string GenerateRandomEmail() =>
        string.Concat(GenerateRandomName(), "@test.com");
}


//public static class StoreTestHelpers
//{
//    public static async Task<Guid> CreateTestStoreAsync(
//        WebApplicationFactory<Program> factory,
//        string token,
//        string name = "Test Store",
//        string description = "Test Description")
//    {
//        using var client = AuthTestHelpers.CreateAuthenticatedClient(factory, token);
//        var response = await client.PostAsJsonAsync("store/register", new CreateStoreRequest(name, description));
//        response.EnsureSuccessStatusCode();

//        var store = await response.Content.ReadFromJsonAsync<StoreDto>();
//        return store.Id;
//    }

//    public static async Task CreateStoreItemAsync(
//        WebApplicationFactory<Program> factory,
//        string token,
//        Guid storeId,
//        CreateItemRequest itemRequest)
//    {
//        using var client = AuthTestHelpers.CreateAuthenticatedClient(factory, token);
//        var response = await client.PostAsJsonAsync($"/store/{storeId}/items", itemRequest);
//        response.EnsureSuccessStatusCode();
//    }
//}