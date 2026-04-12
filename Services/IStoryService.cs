using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Models;

namespace StoryMakerApi.Services;

public interface IStoryService
{
    Task<Result<StoryResponse>> CreateAsync(CreateStoryRequest request, User author, CancellationToken cancellationToken);
    Task<Result<StoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<StoryResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<StoryResponse>> GetByAuthorAsync(int authorId, CancellationToken cancellationToken);
    Task<Result<StoryResponse>> UpdateAsync(int id, UpdateStoryRequest request, User author, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int id, User author, CancellationToken cancellationToken);
}
