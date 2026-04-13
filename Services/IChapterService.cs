using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Chapter;

namespace StoryMakerApi.Services;

public interface IChapterService
{
    Task<Result<ChapterResponse>> CreateAsync(int storyId, CreateChapterRequest request, int authorId, CancellationToken cancellationToken);
    Task<Result<ChapterResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PagedResponse<ChapterResponse>> GetByStoryIdAsync(int storyId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int id, int authorId, CancellationToken cancellationToken);
}
