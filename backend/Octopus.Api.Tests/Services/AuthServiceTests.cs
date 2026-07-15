using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Octopus.Api.Data;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Services;
using Octopus.Api.Tests.Helpers;

namespace Octopus.Api.Tests.Services;

public class AuthServiceTests
{
    private static void SeedUser(AppDbContext ctx, string fullName = "Test User", string username = "testuser", string password = "password123")
    {
        var user = new AppUser
        {
            FullName = fullName,
            Username = username,
            PasswordHash = AuthService.HashPassword(password),
            Role = "Operator",
            CreatedAtUtc = DateTime.UtcNow
        };
        ctx.Users.Add(user);
        ctx.SaveChanges();
    }

    // 1. Register_ValidRequest_ShouldCreateUser
    [Fact]
    public async Task Register_ValidRequest_ShouldCreateUser()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        var req = new RegisterRequest
        {
            FullName = "John Doe",
            Username = "johndoe",
            Password = "secure123"
        };

        var (response, err) = await service.Register(req);

        Assert.Null(err);
        Assert.NotNull(response);
        var userInDb = ctx.Users.FirstOrDefault(u => u.Username == "johndoe");
        Assert.NotNull(userInDb);
        Assert.Equal("John Doe", userInDb.FullName);
        Assert.Equal("Operator", userInDb.Role);
    }

    // 2. Register_ValidRequest_ShouldReturnAuthResponse
    [Fact]
    public async Task Register_ValidRequest_ShouldReturnAuthResponse()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        var req = new RegisterRequest
        {
            FullName = "Jane Smith",
            Username = "janesmith",
            Password = "mypassword"
        };

        var (response, err) = await service.Register(req);

        Assert.Null(err);
        Assert.NotNull(response);
        Assert.Equal("Jane Smith", response.FullName);
        Assert.Equal("janesmith", response.Username);
        Assert.Equal("Operator", response.Role);
        Assert.True(response.Id > 0);
        Assert.True(response.SignedInAtUtc > DateTime.MinValue);
    }

    // 3. Register_DuplicateUsername_ShouldReturnError
    [Fact]
    public async Task Register_DuplicateUsername_ShouldReturnError()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        SeedUser(ctx, username: "existinguser");

        var req = new RegisterRequest
        {
            FullName = "Another User",
            Username = "existinguser",
            Password = "password456"
        };

        var (response, err) = await service.Register(req);

        Assert.Null(response);
        Assert.NotNull(err);
        Assert.Contains("already registered", err);
    }

    // 4. Register_ShouldHashPassword
    [Fact]
    public async Task Register_ShouldHashPassword()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        var plainPassword = "mysecret123";
        var req = new RegisterRequest
        {
            FullName = "Hash Tester",
            Username = "hashtester",
            Password = plainPassword
        };

        var (response, err) = await service.Register(req);

        Assert.Null(err);
        Assert.NotNull(response);

        var userInDb = ctx.Users.First(u => u.Username == "hashtester");
        Assert.NotEqual(plainPassword, userInDb.PasswordHash);
        Assert.Contains(".", userInDb.PasswordHash); // format: iterations.base64salt.base64key
    }

    // 5. Login_ValidCredentials_ShouldReturnAuthResponse
    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnAuthResponse()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        SeedUser(ctx, fullName: "Login User", username: "loginuser", password: "correctpass");

        var req = new LoginRequest
        {
            Username = "loginuser",
            Password = "correctpass"
        };

        var result = await service.Login(req);

        Assert.NotNull(result);
        Assert.Equal("Login User", result.FullName);
        Assert.Equal("loginuser", result.Username);
        Assert.Equal("Operator", result.Role);
        Assert.True(result.Id > 0);
        Assert.True(result.SignedInAtUtc > DateTime.MinValue);
    }

    // 6. Login_WrongPassword_ShouldReturnNull
    [Fact]
    public async Task Login_WrongPassword_ShouldReturnNull()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        SeedUser(ctx, username: "someuser", password: "rightpassword");

        var req = new LoginRequest
        {
            Username = "someuser",
            Password = "wrongpassword"
        };

        var result = await service.Login(req);

        Assert.Null(result);
    }

    // 7. Login_NonExistentUser_ShouldReturnNull
    [Fact]
    public async Task Login_NonExistentUser_ShouldReturnNull()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        var req = new LoginRequest
        {
            Username = "ghostuser",
            Password = "anypassword"
        };

        var result = await service.Login(req);

        Assert.Null(result);
    }

    // 8. Login_CaseInsensitiveUsername_ShouldWork
    [Fact]
    public async Task Login_CaseInsensitiveUsername_ShouldWork()
    {
        using var ctx = TestDbContextFactory.CreateDbContext();
        var service = new AuthService(ctx);

        SeedUser(ctx, username: "CaseUser", password: "testpass");

        var req = new LoginRequest
        {
            Username = "caseuser",
            Password = "testpass"
        };

        var result = await service.Login(req);

        Assert.NotNull(result);
        Assert.Equal("CaseUser", result.Username);
    }

    // 9. HashPassword_ShouldProduceDifferentHashesForSameInput
    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForSameInput()
    {
        var password = "samepassword";

        var hash1 = AuthService.HashPassword(password);
        var hash2 = AuthService.HashPassword(password);

        Assert.NotEqual(hash1, hash2); // different salts produce different hashes
        Assert.StartsWith("100000.", hash1); // verify iteration count is included
        Assert.StartsWith("100000.", hash2);
    }

    // 10. VerifyPassword_CorrectPassword_ShouldReturnTrue
    [Fact]
    public void VerifyPassword_CorrectPassword_ShouldReturnTrue()
    {
        var password = "correcthorsebatterystaple";
        var hash = AuthService.HashPassword(password);

        var result = AuthService.VerifyPassword(password, hash);

        Assert.True(result);
    }

    // 11. VerifyPassword_WrongPassword_ShouldReturnFalse
    [Fact]
    public void VerifyPassword_WrongPassword_ShouldReturnFalse()
    {
        var hash = AuthService.HashPassword("realpassword");

        var result = AuthService.VerifyPassword("wrongpassword", hash);

        Assert.False(result);
    }

    // 12. VerifyPassword_MalformedHash_ShouldReturnFalse
    [Fact]
    public void VerifyPassword_MalformedHash_ShouldReturnFalse()
    {
        var result1 = AuthService.VerifyPassword("password", "not.a.valid.hash.too.many.parts");
        var result2 = AuthService.VerifyPassword("password", "invalidformat");
        var result3 = AuthService.VerifyPassword("password", "abc.salt.key"); // non-numeric iterations

        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
    }
}
