using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Models;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;

namespace StoryMakerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StoryController : ControllerBase
{
    private readonly IStoryService _storyService;
    private readonly IUserRepository _userRepository;

    public StoryController(IStoryService storyService, IUserRepository userRepository)
    {
        _storyService = storyService;
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _storyService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<StoryResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _storyService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpPost]
    public async Task<ActionResult<StoryResponse>> Create(
        [FromBody] CreateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _storyService.CreateAsync(request, user, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StoryResponse>> Update(
        int id,
        [FromBody] UpdateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _storyService.UpdateAsync(id, request, user, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _storyService.DeleteAsync(id, user, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var user = await _userRepository.FindByIdAsync(userId, cancellationToken);
        return user ?? throw new UnauthorizedAccessException("User not found.");
    }
}
