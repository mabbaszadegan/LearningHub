using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using Xunit;

namespace EduTrack.Tests.Integration;

public class LoginIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LoginIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldRedirectToHome()
    {
        // Arrange
        var loginData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", "admin@local"),
            new KeyValuePair<string, string>("Password", "Passw0rd!"),
            new KeyValuePair<string, string>("RememberMe", "false")
        });

        // Act
        var response = await _client.PostAsync("/Account/Login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Home", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnLoginPage()
    {
        // Arrange
        var loginData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Email", "invalid@test.com"),
            new KeyValuePair<string, string>("Password", "wrongpassword"),
            new KeyValuePair<string, string>("RememberMe", "false")
        });

        // Act
        var response = await _client.PostAsync("/Account/Login", loginData);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid login attempt", content);
    }
}
