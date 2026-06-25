using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.DTOs;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class AuthService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponse?> Login(LoginRequest request)
    {
        var username = request.Username.Trim();
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        return ToResponse(user);
    }

    public async Task<(AuthResponse? User, string? Error)> Register(RegisterRequest request)
    {
        var fullName = request.FullName.Trim();
        var username = request.Username.Trim();

        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
        {
            return (null, "This username is already registered.");
        }

        var user = new AppUser
        {
            FullName = fullName,
            Username = username,
            PasswordHash = HashPassword(request.Password),
            Role = "Operator",
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (ToResponse(user), null);
    }

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var expectedKey = Convert.FromBase64String(parts[2]);
        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    private static AuthResponse ToResponse(AppUser user)
    {
        return new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Role = user.Role,
            SignedInAtUtc = DateTime.UtcNow
        };
    }
}
