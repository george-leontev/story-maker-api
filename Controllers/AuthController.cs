using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos.Auth;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[SwaggerTag("Аутентификация — регистрация и вход пользователя")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Регистрация нового пользователя",
        Description = "Создаёт учётную запись пользователя и возвращает JWT-токен для аутентификации.",
        OperationId = "Register")]
    [SwaggerResponse(200, "Регистрация прошла успешно. Возвращает JWT-токен и время истечения.", typeof(AuthResponse))]
    [SwaggerResponse(400, "Ошибка валидации или такой username/email уже занят")]
    public async Task<ActionResult<AuthResponse>> Register(
        [SwaggerParameter("Данные для регистрации", Required = true)] [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Вход в систему",
        Description = "Проверяет учётные данные и возвращает JWT-токен.",
        OperationId = "Login")]
    [SwaggerResponse(200, "Вход выполнен успешно. Возвращает JWT-токен и время истечения.", typeof(AuthResponse))]
    [SwaggerResponse(400, "Неверный email или пароль")]
    public async Task<ActionResult<AuthResponse>> Login(
        [SwaggerParameter("Учётные данные", Required = true)] [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}
