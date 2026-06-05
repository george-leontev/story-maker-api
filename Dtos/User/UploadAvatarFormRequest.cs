using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Dtos.User;

public sealed class UploadAvatarFormRequest
{
    [FromForm(Name = "avatar")]
    [SwaggerSchema("Изображение аватара")]
    public IFormFile? Avatar { get; set; }
}
