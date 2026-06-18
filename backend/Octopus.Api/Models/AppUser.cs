namespace Octopus.Api.Models;

public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Operator";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
