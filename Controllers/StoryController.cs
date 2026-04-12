using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[SwaggerTag("Управление историями — создание, чтение, обновление и удаление")]
public class StoryController : BaseController
{
    private readonly IStoryService _storyService;

    public StoryController(IUserRepository userRepository, IStoryService storyService)
        : base(userRepository) => _storyService = storyService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Список всех историй",
        Description = "Возвращает все истории, отсортированные по дате создания (сначала новые).",
        OperationId = "GetAllStories")]
    [SwaggerResponse(200, "Список историй успешно получен", typeof(IReadOnlyList<StoryResponse>))]
    public async Task<ActionResult<IReadOnlyList<StoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _storyService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Получить историю по ID",
        Description = "Возвращает одну историю, включая количество глав и информацию об авторе.",
        OperationId = "GetStoryById")]
    [SwaggerResponse(200, "История найдена", typeof(StoryResponse))]
    [SwaggerResponse(404, "История не найдена")]
    public async Task<ActionResult<StoryResponse>> GetById(
        [SwaggerParameter("ID истории", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var result = await _storyService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Создать новую историю",
        Description = "Создаёт историю для аутентифицированного пользователя.",
        OperationId = "CreateStory")]
    [SwaggerResponse(201, "История создана", typeof(StoryResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    public async Task<ActionResult<StoryResponse>> Create(
        [SwaggerParameter("Детали истории", Required = true)] [FromBody] CreateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _storyService.CreateAsync(request, user, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Обновить историю",
        Description = "Обновляет название и описание. Только автор может редактировать.",
        OperationId = "UpdateStory")]
    [SwaggerResponse(200, "История обновлена", typeof(StoryResponse))]
    [SwaggerResponse(400, "Ошибка валидации или пользователь не является автором")]
    public async Task<ActionResult<StoryResponse>> Update(
        [SwaggerParameter("ID истории", Required = true)] int id,
        [SwaggerParameter("Обновлённые данные", Required = true)] [FromBody] UpdateStoryRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _storyService.UpdateAsync(id, request, user, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:int}")]
    [SwaggerOperation(
        Summary = "Удалить историю",
        Description = "Удаляет историю и все её главы. Только автор может удалять.",
        OperationId = "DeleteStory")]
    [SwaggerResponse(204, "История удалена")]
    [SwaggerResponse(400, "Пользователь не является автором")]
    public async Task<ActionResult> Delete(
        [SwaggerParameter("ID истории", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var result = await _storyService.DeleteAsync(id, user, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error });
    }
}
