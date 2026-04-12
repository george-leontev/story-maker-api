using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos.Choice;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/chapters/{chapterId}/choices")]
[ApiController]
[SwaggerTag("Голосование за выбор в интерактивной истории")]
public class ChoiceController : BaseController
{
    private readonly IChoiceService _choiceService;

    public ChoiceController(IUserRepository userRepository, IChoiceService choiceService)
        : base(userRepository) => _choiceService = choiceService;

    [HttpPost]
    [Authorize]
    [SwaggerOperation(
        Summary = "Создать бинарный выбор",
        Description = "Создаёт выбор из двух вариантов для главы с таймером. Только автор истории может создавать выборы. " +
                      "Каждая глава может иметь не более одного активного выбора.",
        OperationId = "CreateChoice")]
    [SwaggerResponse(200, "Выбор создан", typeof(ChoiceResponse))]
    [SwaggerResponse(400, "Ошибка валидации, пользователь не является автором или у главы уже есть активный выбор")]
    public async Task<ActionResult<ChoiceResponse>> Create(
        [SwaggerParameter("ID родительской главы", Required = true)] int chapterId,
        [SwaggerParameter("Тексты вариантов и длительность в минутах", Required = true)] [FromBody] CreateChoiceRequest request,
        CancellationToken cancellationToken)
    {
        var authorId = GetCurrentUserId();
        var result = await _choiceService.CreateAsync(chapterId, request, authorId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:int}")]
    [SwaggerOperation(
        Summary = "Получить выбор (просмотр читателем)",
        Description = "Возвращает варианты выбора и время истечения. Счётчики голосов скрыты, пока выбор активен — " +
                      "они отображаются только после истечения таймера (IsClosed = true).",
        OperationId = "GetChoice")]
    [SwaggerResponse(200, "Выбор найден. Счётчики голосов могут быть null, если выбор ещё активен.", typeof(ChoiceResponse))]
    [SwaggerResponse(404, "Выбор не найден")]
    public async Task<ActionResult<ChoiceResponse>> Get(
        [SwaggerParameter("ID родительской главы", Required = true)] int chapterId,
        [SwaggerParameter("ID выбора", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var result = await _choiceService.GetPublicAsync(id, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:int}/author")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Получить выбор с текущими голосами (просмотр автором)",
        Description = "Возвращает выбор с текущими счётчиками голосов, видимыми только автору истории.",
        OperationId = "GetChoiceAuthorView")]
    [SwaggerResponse(200, "Просмотр автора с текущими счётчиками голосов", typeof(ChoiceAuthorResponse))]
    [SwaggerResponse(403, "Пользователь не является автором истории")]
    public async Task<ActionResult<ChoiceAuthorResponse>> GetAuthorView(
        [SwaggerParameter("ID родительской главы", Required = true)] int chapterId,
        [SwaggerParameter("ID выбора", Required = true)] int id,
        CancellationToken cancellationToken)
    {
        var authorId = GetCurrentUserId();
        var result = await _choiceService.GetAuthorViewAsync(id, authorId, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Forbid();
    }

    [HttpPost("{id:int}/vote")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Проголосовать",
        Description = "Записывает голос за вариант 1 или 2. Каждый пользователь может проголосовать только один раз. " +
                      "Счётчики голосов скрыты до истечения таймера.",
        OperationId = "Vote")]
    [SwaggerResponse(200, "Голос успешно записан")]
    [SwaggerResponse(400, "Пользователь уже проголосовал, выбор закрыт или истёк")]
    public async Task<ActionResult> Vote(
        [SwaggerParameter("ID родительской главы", Required = true)] int chapterId,
        [SwaggerParameter("ID выбора", Required = true)] int id,
        [SwaggerParameter("Вариант для голосования: 1 или 2", Required = true)] [FromBody] VoteRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _choiceService.VoteAsync(id, request.Option, userId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Голос записан." })
            : BadRequest(new { error = result.Error });
    }
}

public sealed record VoteRequest(int Option);
