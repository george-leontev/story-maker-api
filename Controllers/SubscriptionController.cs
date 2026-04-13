using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Dtos.Subscription;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/stories/{storyId}")]
[ApiController]
[SwaggerTag("Подписки на истории")]
public class SubscriptionController : BaseController
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(IUserRepository userRepository, ISubscriptionService subscriptionService)
        : base(userRepository) => _subscriptionService = subscriptionService;

    [HttpPost("subscribe")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Подписаться на историю",
        Description = "Добавляет подписку на историю. Требуется аутентификация.",
        OperationId = "Subscribe")]
    [SwaggerResponse(200, "Подписка оформлена")]
    [SwaggerResponse(400, "Уже подписаны или история не найдена")]
    public async Task<ActionResult> Subscribe(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.SubscribeAsync(storyId, userId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Вы подписались на историю." })
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete("unsubscribe")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Отписаться от истории",
        Description = "Удаляет подписку на историю. Требуется аутентификация.",
        OperationId = "Unsubscribe")]
    [SwaggerResponse(200, "Подписка удалена")]
    [SwaggerResponse(400, "Не подписаны или история не найдена")]
    public async Task<ActionResult> Unsubscribe(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.UnsubscribeAsync(storyId, userId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Вы отписались от истории." })
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("subscribers")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Список подписчиков истории",
        Description = "Возвращает всех подписчиков истории. Только автор истории может просматривать.",
        OperationId = "GetSubscribers")]
    [SwaggerResponse(200, "Список подписчиков успешно получен", typeof(IReadOnlyList<SubscriptionResponse>))]
    [SwaggerResponse(400, "Пользователь не является автором истории")]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult<IReadOnlyList<SubscriptionResponse>>> GetSubscribers(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.GetSubscribersAsync(storyId, userId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("subscribed")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Мои подписки",
        Description = "Возвращает истории, на которые подписан текущий пользователь, с пагинацией.",
        OperationId = "GetSubscribedStories")]
    [SwaggerResponse(200, "Список подписок успешно получен", typeof(PagedResponse<StoryResponse>))]
    public async Task<ActionResult<PagedResponse<StoryResponse>>> GetSubscribedStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _subscriptionService.GetSubscribedStoriesAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }
}
