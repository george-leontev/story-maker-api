using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Choice;

namespace StoryMakerApi.Services;

public interface IChoiceService
{
    Task<Result<ChoiceResponse>> CreateAsync(int chapterId, CreateChoiceRequest request, int authorId, CancellationToken cancellationToken);
    Task<Result<ChoiceResponse>> GetPublicAsync(int id, CancellationToken cancellationToken);
    Task<Result<ChoiceAuthorResponse>> GetAuthorViewAsync(int id, int authorId, CancellationToken cancellationToken);
    Task<Result<bool>> VoteAsync(int choiceId, int option, int userId, CancellationToken cancellationToken);
}
