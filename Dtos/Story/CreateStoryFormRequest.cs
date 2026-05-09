using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace StoryMakerApi.Dtos.Story;

public sealed class CreateStoryFormRequest
{
    [FromForm(Name = "title")]
    [SwaggerSchema("Заголовок истории")]
    public string Title { get; set; } = string.Empty;

    [FromForm(Name = "description")]
    [SwaggerSchema("Описание истории")]
    public string Description { get; set; } = string.Empty;

    [FromForm(Name = "coverImage")]
    [SwaggerSchema("Обложка истории (опционально)")]
    public IFormFile? CoverImage { get; set; }
}
