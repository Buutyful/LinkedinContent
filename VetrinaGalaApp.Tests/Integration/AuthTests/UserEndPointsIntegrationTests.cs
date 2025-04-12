using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VetrinaGalaApp.ApiService.EndPoints;

namespace VetrinaGalaApp.Tests.Integration.AuthTests;

public class UserEndpointsIntegrationTests(IntegrationTestBase integrationTestBase) :
    IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _base = integrationTestBase;  

    [Fact]
    public async Task Register_WithValidCredentials_CreatesUser()
    {
        // Arrange        
        var request = new RegisterAsUserRequest(
            "testuser", "test@example.com", "SecurePassword123!");

        // Act
        var response = await _base.Client.PostAsJsonAsync("/auth/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var userManager = _base.Factory.CreateUserManager();
        var user = await userManager.FindByEmailAsync(request.Email);

        Assert.NotNull(user);
        Assert.True(await userManager.CheckPasswordAsync(user, request.Password));
        Assert.Equal(request.UserName, user.UserName);
    }
    [Fact]
    public async Task Register_WithExistingCredentials_ReturnsConflict()
    {
        // Arrange        
        var request = new RegisterAsUserRequest(
            "testuser2", "test2@example.com", "SecurePassword123!");

        // Act
        var response = await _base.Client.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();

        // Assert      
        var response2 = await _base.Client.PostAsJsonAsync("/auth/register", request);
        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsValidationError()
    {
        // Arrange        
        var request = new RegisterAsUserRequest(
            "testuser1", "test1@example.com", "weak");

        // Act
        var response = await _base.Client.PostAsJsonAsync("/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Password must be at least 6 characters", content);
    }

    [Fact]
    public async Task Register_WithValidCredentials_ReturnsCorrectJwt()
    {
        // Arrange        
        var request = new RegisterAsUserRequest(
          "testuser3", "test3@example.com", "SecurePassword123!");

        // Act
        var response = await _base.Client.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(result);
        Assert.NotNull(result.Token);

        using var client = _base.Factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", result.Token);

        var check = await client.GetAsync("/auth/check");

        // Assert
        Assert.True(check.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, check.StatusCode);

    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsCorrectJwt()
    {
        // Arrange        
        var registerRequest = new RegisterAsUserRequest(
            "testuser4", "test4@example.com", "SecurePassword123!");

        // Act - First register the user
        var registerResponse = await _base.Client.PostAsJsonAsync("/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<AuthenticationResult>();
        Assert.NotNull(registerResult);

        // Act - Then login with the registered credentials
        var loginResponse = await _base.Client.PostAsJsonAsync(
            "/auth/login", new AuthenticationRequest(registerResult.Email, "SecurePassword123!"));
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthenticationResult>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);

        // Act - Verify protected endpoint access with login token
        using var client = _base.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);
        var check = await client.GetAsync("/auth/check");

        // Assert
        Assert.True(check.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.OK, check.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new AuthenticationRequest("wrong@example.com", "WrongPassword123!");

        // Act
        var response = await _base.Client.PostAsJsonAsync("/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _base.Factory.CreateClient();

        // Act
        var response = await client.GetAsync("/auth/check");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, "invalid-token");

        // Act
        var response = await client.GetAsync("/auth/check");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorize2d()
    {
        // Arrange
        using var client = AuthTestHelpers.CreateAuthenticatedClient(_base.Factory, "invalid-token");

        // Act - Add a handler to see redirect information
        var handler = new HttpClientHandler();
        handler.AllowAutoRedirect = false; // Prevent auto-redirect to see what's happening
        var clientWithoutRedirect = _base.Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
        clientWithoutRedirect.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await clientWithoutRedirect.GetAsync("/auth/check");

        // Assert
        Console.WriteLine($"Status code: {response.StatusCode}");
        Console.WriteLine($"Headers: {string.Join(", ", response.Headers)}");
        if (response.Headers.Location != null)
        {
            Console.WriteLine($"Redirect location: {response.Headers.Location}");
        }

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public static class UserTestHelpers
{
    public static async Task<string> RegisterTestUserAsync(
        HttpClient client,
        string username = "testuser",
        string email = "test@example.com",
        string password = "SecurePassword123!")
    {
        var request = new RegisterAsUserRequest(username, email, password);
        var response = await client.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

        return result!.Token;
    }
}
public static class AuthTestHelpers
{
    public static HttpClient CreateAuthenticatedClient(
        WebApplicationFactory<ProgramApiMarker> factory,
        string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}