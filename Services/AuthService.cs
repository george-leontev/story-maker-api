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

    public AuthService(
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Result<AuthResponse>.Failure("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<AuthResponse>.Failure("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return Result<AuthResponse>.Failure("Password must be at least 6 characters.");
        }

        if (!request.Email.Contains("@"))
        {
            return Result<AuthResponse>.Failure("Invalid email format.");
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Email is already registered.");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
        {
            return Result<AuthResponse>.Failure("Username is already taken.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
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
            return Result<AuthResponse>.Failure("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("Password is required.");
        }

        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure("Invalid email or password.");
        }

        _logger.LogInformation("User logged in: {UserId} ({Email})", user.Id, user.Email);

        return Result<AuthResponse>.Success(GenerateAuthResponse(user));
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var (token, expiresAt) = JwtHelper.GenerateToken(user, _jwtSettings);
        return new AuthResponse(token, expiresAt);
    }
}
