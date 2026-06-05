using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos;
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
    private readonly ILogger<StoryController> _logger;

    public StoryController(IUserRepository userRepository, IStoryService storyService, ILogger<StoryController> logger)
        : base(userRepository)
    {
        _storyService = storyService;
        _logger = logger;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Список всех историй",
        Description = "Возвращает истории с пагинацией, отсортированные по дате создания (сначала новые).",
        OperationId = "GetAllStories")]
    [SwaggerResponse(200, "Список историй успешно получен", typeof(PagedResponse<StoryResponse>))]
    public async Task<ActionResult<PagedResponse<StoryResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _storyService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [SwaggerOperation(
        Summary = "Мои истории",
        Description = "Возвращает истории текущего пользователя с пагинацией.",
        OperationId = "GetMyStories")]
    [SwaggerResponse(200, "Список моих историй", typeof(PagedResponse<StoryResponse>))]
    public async Task<ActionResult<PagedResponse<StoryResponse>>> GetMyStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _storyService.GetByAuthorAsync(userId, page, pageSize, cancellationToken);
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
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Создать новую историю",
        Description = "Создаёт историю для аутентифицированного пользователя. Можно прикрепить обложку.",
        OperationId = "CreateStory")]
    [SwaggerResponse(201, "История создана", typeof(StoryResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    public async Task<ActionResult<StoryResponse>> Create(
        [FromForm] CreateStoryFormRequest form,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("CreateStory called: title={Title}, description={DescLength}, coverImage={CoverSize}",
            form.Title, form.Description?.Length, form.CoverImage?.Length);

        if (string.IsNullOrWhiteSpace(form.Title))
            return BadRequest(new { error = "Заголовок истории обязателен." });

        if (string.IsNullOrWhiteSpace(form.Description))
            return BadRequest(new { error = "Описание истории обязательно." });

        try
        {
            var user = await GetCurrentUserAsync(cancellationToken);
            var request = new CreateStoryRequest(form.Title!, form.Description!);
            var result = await _storyService.CreateAsync(request, user, form.CoverImage, cancellationToken);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : BadRequest(new { error = result.Error });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating story");
            return BadRequest(new { error = "Внутренняя ошибка сервера: " + ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Обновить историю",
        Description = "Обновляет название, описание и обложку. Только автор может редактировать.",
        OperationId = "UpdateStory")]
    [SwaggerResponse(200, "История обновлена", typeof(StoryResponse))]
    [SwaggerResponse(400, "Ошибка валидации или пользователь не является автором")]
    public async Task<ActionResult<StoryResponse>> Update(
        [SwaggerParameter("ID истории", Required = true)] int id,
        [FromForm] UpdateStoryFormRequest form,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        var request = new UpdateStoryRequest(form.Title, form.Description);
        var result = await _storyService.UpdateAsync(id, request, user, form.CoverImage, cancellationToken);
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
