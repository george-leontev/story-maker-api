using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos.Comment;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/stories/{storyId}/comments")]
[ApiController]
[SwaggerTag("Комментарии к историям")]
public class CommentController : BaseController
{
    private readonly ICommentService _commentService;

    public CommentController(IUserRepository userRepository, ICommentService commentService)
        : base(userRepository) => _commentService = commentService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Список комментариев истории",
        Description = "Возвращает все комментарии к истории, отсортированные по времени (сначала новые).",
        OperationId = "GetComments")]
    [SwaggerResponse(200, "Список комментариев успешно получен", typeof(IReadOnlyList<CommentResponse>))]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetAll(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        CancellationToken cancellationToken)
    {
        var result = await _commentService.GetByStoryIdAsync(storyId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize]
    [SwaggerOperation(
        Summary = "Добавить комментарий",
        Description = "Создаёт комментарий к истории. Требуется аутентификация.",
        OperationId = "CreateComment")]
    [SwaggerResponse(201, "Комментарий создан", typeof(CommentResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult<CommentResponse>> Create(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        [SwaggerParameter("Текст комментария", Required = true)] [FromBody] CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _commentService.CreateAsync(storyId, request, user, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAll), new { storyId }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Удалить комментарий",
        Description = "Удаляет комментарий. Только автор комментария может удалить.",
        OperationId = "DeleteComment")]
    [SwaggerResponse(204, "Комментарий удалён")]
    [SwaggerResponse(400, "Пользователь не является автором комментария")]
    [SwaggerResponse(404, "Комментарий не найден")]
    public async Task<ActionResult> Delete(
        [SwaggerParameter("ID истории", Required = true)] int storyId,
        [SwaggerParameter("ID комментария", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _commentService.DeleteAsync(id, userId, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }
}
