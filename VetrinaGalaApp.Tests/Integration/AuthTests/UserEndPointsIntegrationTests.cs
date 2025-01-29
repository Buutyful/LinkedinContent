using Docker.DotNet.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using VetrinaGalaApp.ApiService.EndPoints;

namespace VetrinaGalaApp.Tests.Integration.AuthTests;

public class UserEndpointsIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Register_WithValidCredentials_CreatesUserWithHashedPassword()
    {
        // Arrange        
        var request = new RegisterAsUserRequest(
            "testuser", "test@example.com", "SecurePassword123!");

        // Act
        var response = await Client.PostAsJsonAsync("/auth/register", request);

        // Assert
        response.EnsureSuccessStatusCode();
        
        var userManager = Factory.CreateUserManager();
        var user = await userManager.FindByEmailAsync(request.Email);

        Assert.NotNull(user);
        Assert.True(await userManager.CheckPasswordAsync(user, request.Password));
        Assert.Equal(request.UserName, user.UserName);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsValidationError()
    {
        // Arrange        
        var request = new RegisterAsUserRequest(
            "testuser", "test@example.com", "weak");

        // Act
        var response = await Client.PostAsJsonAsync("/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Password must be at least 6 characters", content);
    }
}