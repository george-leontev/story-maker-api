using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;

namespace StoryMakerApi.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    protected BaseController(IUserRepository userRepository) => _userRepository = userRepository;

    /// <summary>
    /// Extracts the current user's ID from the JWT claim.
    /// Throws <see cref="UnauthorizedAccessException"/> if the claim is missing or invalid.
    /// </summary>
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : throw new UnauthorizedAccessException();
    }

    /// <summary>
    /// Resolves the current user from the database.
    /// Throws <see cref="UnauthorizedAccessException"/> if the user is not found.
    /// </summary>
    protected async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        return user ?? throw new UnauthorizedAccessException("User not found.");
    }
}
