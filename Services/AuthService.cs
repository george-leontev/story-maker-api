using Microsoft.Extensions.Options;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Auth;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;
using StoryMakerApi.Settings;
using StoryMakerApi.Utils;

namespace StoryMakerApi.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;
    private readonly IWebHostEnvironment _env;

    public AuthService(
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger,
        IWebHostEnvironment env)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _env = env;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Result<AuthResponse>.Failure("Имя пользователя обязательно.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<AuthResponse>.Failure("Email обязателен.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return Result<AuthResponse>.Failure("Пароль должен содержать не менее 6 символов.");
        }

        if (!request.Email.Contains("@"))
        {
            return Result<AuthResponse>.Failure("Неверный формат email.");
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Этот email уже зарегистрирован.");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Это имя пользователя уже занято.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        string? avatarImageUrl = null;
        if (request.Avatar != null)
        {
            avatarImageUrl = await SaveAvatarAsync(request.Avatar, cancellationToken);
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            AvatarImageUrl = avatarImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User registered: {UserId} ({Username})", user.Id, user.Username);

        return Result<AuthResponse>.Success(GenerateAuthResponse(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<AuthResponse>.Failure("Email обязателен.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Пароль обязателен.");
        }

        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Неверный email или пароль.");
        }

        _logger.LogInformation("User logged in: {UserId} ({Email})", user.Id, user.Email);

        return Result<AuthResponse>.Success(GenerateAuthResponse(user));
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var (token, expiresAt) = JwtHelper.GenerateToken(user, _jwtSettings);
        return new AuthResponse(token, expiresAt);
    }

    private async Task<string> SaveAvatarAsync(IFormFile avatar, CancellationToken cancellationToken)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var fileExtension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
            throw new ArgumentException("Разрешены только изображения: JPG, PNG, WEBP");

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_env.ContentRootPath, "data", "avatars", fileName);

        Directory.CreateDirectory(Path.Combine(_env.ContentRootPath, "data", "avatars"));

        await using var stream = new FileStream(filePath, FileMode.Create);
        await avatar.CopyToAsync(stream, cancellationToken);

        return $"/data/avatars/{fileName}";
    }
}
