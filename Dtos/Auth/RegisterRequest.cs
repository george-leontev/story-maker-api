using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Dtos.Auth;

public sealed class RegisterRequest
{
    [FromForm(Name = "username")]
    [SwaggerSchema("Имя пользователя")]
    public string Username { get; set; } = string.Empty;

    [FromForm(Name = "email")]
    [SwaggerSchema("Email")]
    public string Email { get; set; } = string.Empty;

    [FromForm(Name = "password")]
    [SwaggerSchema("Пароль")]
    public string Password { get; set; } = string.Empty;

    [FromForm(Name = "avatar")]
    [SwaggerSchema("Аватар (опционально)")]
    public IFormFile? Avatar { get; set; }
}
