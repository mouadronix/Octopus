using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Octopus.Api.DTOs;
using Octopus.Api.Tests.Helpers;
using Xunit;

namespace Octopus.Api.Tests.Controllers;

public class AuthControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthControllerTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // 1. Register with a valid request should return 200 OK with an AuthResponse
    [Fact]
    public async Task Register_ValidRequest_ShouldReturnOk()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Username = "johndoe",
            Password = "secret123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(auth);
        Assert.True(auth.Id > 0);
        Assert.Equal("John Doe", auth.FullName);
        Assert.Equal("johndoe", auth.Username);
        Assert.Equal("Operator", auth.Role);
        Assert.True(auth.SignedInAtUtc > DateTime.MinValue);
    }

    // 2. Register with a duplicate username should return 400 BadRequest
    [Fact]
    public async Task Register_DuplicateUsername_ShouldReturnBadRequest()
    {
        var request = new RegisterRequest
        {
            FullName = "Jane Doe",
            Username = "janedoe",
            Password = "secret123"
        };

        // First registration should succeed
        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request, _jsonOptions);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Second registration with the same username should fail
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", request, _jsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    // 3. Register with invalid model data should return 400 ValidationProblem
    [Fact]
    public async Task Register_InvalidModel_ShouldReturnValidationProblem()
    {
        // Send request with empty FullName (violates [Required])
        var invalidRequest = new
        {
            FullName = "",
            Username = "validuser",
            Password = "secret123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", invalidRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 4. Login with valid credentials should return 200 OK with an AuthResponse
    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnOk()
    {
        // Register a user first
        var registerRequest = new RegisterRequest
        {
            FullName = "Login User",
            Username = "loginuser",
            Password = "secret123"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest, _jsonOptions);

        // Now login with correct credentials
        var loginRequest = new LoginRequest
        {
            Username = "loginuser",
            Password = "secret123"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(auth);
        Assert.True(auth.Id > 0);
        Assert.Equal("Login User", auth.FullName);
        Assert.Equal("loginuser", auth.Username);
        Assert.Equal("Operator", auth.Role);
        Assert.True(auth.SignedInAtUtc > DateTime.MinValue);
    }

    // 5. Login with the wrong password should return 401 Unauthorized
    [Fact]
    public async Task Login_WrongPassword_ShouldReturnUnauthorized()
    {
        // Register a user first
        var registerRequest = new RegisterRequest
        {
            FullName = "Wrong Pwd User",
            Username = "wrongpwduser",
            Password = "correctpassword"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest, _jsonOptions);

        // Login with the wrong password
        var loginRequest = new LoginRequest
        {
            Username = "wrongpwduser",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 6. Login with a non-existent user should return 401 Unauthorized
    [Fact]
    public async Task Login_NonExistentUser_ShouldReturnUnauthorized()
    {
        var loginRequest = new LoginRequest
        {
            Username = "doesnotexist",
            Password = "anypassword"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // 7. End-to-end: register then login should work correctly
    [Fact]
    public async Task Register_ThenLogin_ShouldWork()
    {
        // Register
        var registerRequest = new RegisterRequest
        {
            FullName = "E2E User",
            Username = "e2euser",
            Password = "e2epassword123"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest, _jsonOptions);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(registered);
        Assert.Equal("E2E User", registered.FullName);
        Assert.Equal("e2euser", registered.Username);

        // Login with the same credentials
        var loginRequest = new LoginRequest
        {
            Username = "e2euser",
            Password = "e2epassword123"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest, _jsonOptions);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loggedIn = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(_jsonOptions);
        Assert.NotNull(loggedIn);
        Assert.Equal(registered.Id, loggedIn.Id);
        Assert.Equal(registered.FullName, loggedIn.FullName);
        Assert.Equal(registered.Username, loggedIn.Username);
        Assert.Equal(registered.Role, loggedIn.Role);
    }
}
