using System.Net.Http.Json;
using VetrinaGalaApp.ApiService.EndPoints;

namespace VetrinaGalaApp.Tests.Integration.AuthTests;

public class UserEndpointsIntegrationTests(IntegrationTestBase integrationTestBase) :
    IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _base = integrationTestBase;
    private static readonly RegisterAsUserRequest[] _users =
    [
        new RegisterAsUserRequest("testuser", "test@example.com", "SecurePassword123!"),
        new RegisterAsUserRequest("testuser1", "test1@example.com", "weak"),
    ];

    [Fact]
    public async Task Register_WithValidCredentials_CreatesUserWithHashedPassword()
    {
        // Arrange        
        var request = _users[0];

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
    public async Task Register_WithWeakPassword_ReturnsValidationError()
    {
        // Arrange        
        var request = _users[1];

        // Act
        var response = await _base.Client.PostAsJsonAsync("/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Password must be at least 6 characters", content);
    }
}