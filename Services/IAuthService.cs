using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Auth;

namespace StoryMakerApi.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
