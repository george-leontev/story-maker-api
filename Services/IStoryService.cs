using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Story;
using StoryMakerApi.Models;

namespace StoryMakerApi.Services;

public interface IStoryService
{
    Task<Result<StoryResponse>> CreateAsync(CreateStoryRequest request, User author, CancellationToken cancellationToken);
    Task<Result<StoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagedResponse<StoryResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<StoryResponse>> GetByAuthorAsync(int authorId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Result<StoryResponse>> UpdateAsync(int id, UpdateStoryRequest request, User author, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int id, User author, CancellationToken cancellationToken);
}
