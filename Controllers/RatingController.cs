using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Rating;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/stories/{storyId}/rating")]
[ApiController]
[SwaggerTag("Рейтинг историй — оценка от 1 до 5")]
public class RatingController : BaseController
{
    private readonly IRatingService _ratingService;

    public RatingController(IUserRepository userRepository, IRatingService ratingService)
        : base(userRepository) => _ratingService = ratingService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получить рейтинг истории",
        Description = "Возвращает средний рейтинг и количество оценок. Если пользователь аутентифицирован — также возвращает его оценку.",
        OperationId = "GetStoryRating")]
    [SwaggerResponse(200, "Рейтинг успешно получен", typeof(RatingResponse))]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult<RatingResponse>> Get(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        CancellationToken cancellationToken)
    {
        int? userId = null;
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var id))
            userId = id;

        var result = await _ratingService.GetRatingAsync(storyId, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [SwaggerOperation(
        Summary = "Оценить историю",
        Description = "Ставит оценку от 1 до 5. Если пользователь уже оценивал — оценка обновляется.",
        OperationId = "RateStory")]
    [SwaggerResponse(200, "Оценка сохранена", typeof(RatingResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult<RatingResponse>> Rate(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        [SwaggerParameter("Оценка от 1 до 5", Required = true)] [FromBody] RateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _ratingService.RateAsync(storyId, userId, request.Score, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete]
    [Authorize]
    [SwaggerOperation(
        Summary = "Удалить свою оценку",
        Description = "Удаляет оценку текущего пользователя для данной истории.",
        OperationId = "RemoveRating")]
    [SwaggerResponse(200, "Оценка удалена")]
    [SwaggerResponse(400, "Пользователь не оценивал эту историю")]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult> Remove(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _ratingService.RemoveRatingAsync(storyId, userId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Оценка удалена." })
            : BadRequest(new { error = result.Error });
    }
}
