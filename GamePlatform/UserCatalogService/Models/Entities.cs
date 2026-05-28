using System.ComponentModel.DataAnnotations;

namespace UserCatalogService.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Developer { get; set; } = string.Empty;
}

public class LibraryRecord
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

// DTOшки для контроллеров
public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record CreateGameRequest(string Title, string Genre, string Developer);