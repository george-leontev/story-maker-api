using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.User;
using StoryMakerApi.Repositories;
using StoryMakerApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[SwaggerTag("Управление профилем пользователя")]
public class UserController : BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserRepository userRepository, IUserService userService)
        : base(userRepository) => _userService = userService;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Получить мой профиль",
        Description = "Возвращает данные текущего пользователя со статистикой.",
        OperationId = "GetMyProfile")]
    [SwaggerResponse(200, "Профиль получен", typeof(ProfileResponse))]
    public async Task<ActionResult<ProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var profile = await _userService.GetProfileAsync(userId, cancellationToken);
        return Ok(profile);
    }

    [HttpGet("extended")]
    [SwaggerOperation(
        Summary = "Получить расширенный профиль",
        Description = "Возвращает профиль пользователя с последними историями.",
        OperationId = "GetMyProfileExtended")]
    [SwaggerResponse(200, "Расширенный профиль получен", typeof(UserProfileExtended))]
    public async Task<ActionResult<UserProfileExtended>> GetProfileExtended(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var profile = await _userService.GetProfileExtendedAsync(userId, cancellationToken);
        return Ok(profile);
    }

    [HttpPut]
    [SwaggerOperation(
        Summary = "Обновить профиль",
        Description = "Обновляет имя пользователя и/или email.",
        OperationId = "UpdateProfile")]
    [SwaggerResponse(200, "Профиль обновлен", typeof(ProfileResponse))]
    [SwaggerResponse(400, "Ошибка валидации")]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("password")]
    [SwaggerOperation(
        Summary = "Сменить пароль",
        Description = "Обновляет пароль пользователя. Возвращает обновлённый профиль.",
        OperationId = "ChangePassword")]
    [SwaggerResponse(200, "Пароль изменен", typeof(ProfileResponse))]
    [SwaggerResponse(400, "Неверный текущий пароль или слабый новый пароль")]
    public async Task<ActionResult<ProfileResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.ChangePasswordAsync(userId, request, cancellationToken);
        return result.IsSuccess
            ? Ok(await _userService.GetProfileAsync(userId, cancellationToken))
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Загрузить аватар",
        Description = "Загружает новое изображение аватара. Старый аватар удаляется. Возвращает обновлённый профиль.",
        OperationId = "UploadAvatar")]
    [SwaggerResponse(200, "Аватар загружен", typeof(ProfileResponse))]
    [SwaggerResponse(400, "Неверный формат файла")]
    public async Task<ActionResult<ProfileResponse>> UploadAvatar(
        [FromForm] IFormFile avatar,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        
        if (avatar == null || avatar.Length == 0)
            return BadRequest(new { error = "Файл не выбран." });

        var result = await _userService.UploadAvatarAsync(userId, avatar, cancellationToken);
        return result.IsSuccess
            ? Ok(await _userService.GetProfileAsync(userId, cancellationToken))
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("votes")]
    [SwaggerOperation(
        Summary = "История моих голосований",
        Description = "Возвращает все голоса пользователя с результатами (если выбор закрыт).",
        OperationId = "GetVoteHistory")]
    [SwaggerResponse(200, "История голосований получена", typeof(PagedResponse<VoteHistoryResponse>))]
    public async Task<ActionResult<PagedResponse<VoteHistoryResponse>>> GetVoteHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetVoteHistoryAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("stories")]
    [SwaggerOperation(
        Summary = "Мои истории",
        Description = "Возвращает истории, созданные пользователем.",
        OperationId = "GetMyStories")]
    [SwaggerResponse(200, "Список историй получен", typeof(PagedResponse<StoryListItem>))]
    public async Task<ActionResult<PagedResponse<StoryListItem>>> GetMyStories(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.GetMyStoriesAsync(userId, page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [SwaggerOperation(
        Summary = "Удалить аккаунт",
        Description = "Безвозвратно удаляет аккаунт и все связанные данные.",
        OperationId = "DeleteAccount")]
    [SwaggerResponse(200, "Аккаунт удален")]
    [SwaggerResponse(400, "Ошибка")]
    public async Task<ActionResult> DeleteAccount(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.DeleteAccountAsync(userId, cancellationToken);
        return result.IsSuccess
            ? Ok(new { message = "Аккаунт успешно удален." })
            : BadRequest(new { error = result.Error });
    }
}