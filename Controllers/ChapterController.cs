using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Chapter;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/stories/{storyId}/chapters")]
[ApiController]
[SwaggerTag("Управление главами истории")]
public class ChapterController : BaseController
{
    private readonly IChapterService _chapterService;

    public ChapterController(IUserRepository userRepository, IChapterService chapterService)
        : base(userRepository) => _chapterService = chapterService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Список всех глав истории",
        Description = "Возвращает главы с пагинацией, отсортированные по порядковому номеру.",
        OperationId = "GetAllChapters")]
    [SwaggerResponse(200, "Список глав успешно получен", typeof(PagedResponse<ChapterResponse>))]
    public async Task<ActionResult<PagedResponse<ChapterResponse>>> GetAll(
        [SwaggerParameter("ID родительской истории", Required = true)] int storyId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _chapterService.GetByStoryIdAsync(storyId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Получить главу по ID",
        Description = "Возвращает одну главу с её содержимым.",
        OperationId = "GetChapterById")]
    [SwaggerResponse(200, "Глава найдена", typeof(ChapterResponse))]
    [SwaggerResponse(404, "Глава не найдена")]
    public async Task<ActionResult<ChapterResponse>> GetById(
        [SwaggerParameter("ID родительской истории", Required = true)] int storyId,
        [SwaggerParameter("ID главы", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var result = await _chapterService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize]
    [SwaggerOperation(
        Summary = "Создать новую главу",
        Description = "Добавляет главу к истории. Только автор истории может создавать главы.",
        OperationId = "CreateChapter")]
    [SwaggerResponse(201, "Глава создана", typeof(ChapterResponse))]
    [SwaggerResponse(400, "Ошибка валидации или пользователь не является автором")]
    public async Task<ActionResult<ChapterResponse>> Create(
        [SwaggerParameter("ID родительской истории", Required = true)] int storyId,
        [SwaggerParameter("Содержимое главы и порядковый номер", Required = true)] [FromBody] CreateChapterRequest request,
        CancellationToken cancellationToken)
    {
        var authorId = GetCurrentUserId();
        var result = await _chapterService.CreateAsync(storyId, request, authorId, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { storyId, id = result.Value!.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Удалить главу",
        Description = "Удаляет главу из истории. Только автор истории может удалять.",
        OperationId = "DeleteChapter")]
    [SwaggerResponse(204, "Глава удалена")]
    [SwaggerResponse(400, "Пользователь не является автором")]
    public async Task<ActionResult> Delete(
        [SwaggerParameter("ID родительской истории", Required = true)] int storyId,
        [SwaggerParameter("ID главы", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var authorId = GetCurrentUserId();
        var result = await _chapterService.DeleteAsync(id, authorId, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }
}
