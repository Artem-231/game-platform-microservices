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
public class RegisterRequest
{
    [Required(ErrorMessage = "Поле Username обязательно для заполнения.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Поле Email обязательно для заполнения.")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Поле Password обязательно для заполнения.")]
    [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов.")]
    public string Password { get; set; } = null!;
}

public class LoginRequest
{
    [Required(ErrorMessage = "Поле Email обязательно для заполнения.")]
    [EmailAddress(ErrorMessage = "Некорректный формат Email.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Поле Password обязательно для заполнения.")]
    public string Password { get; set; } = null!;
}
public record CreateGameRequest(string Title, string Genre, string Developer);