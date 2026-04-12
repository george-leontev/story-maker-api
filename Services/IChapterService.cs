using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.Chapter;

namespace StoryMakerApi.Services;

public interface IChapterService
{
    Task<Result<ChapterResponse>> CreateAsync(int storyId, CreateChapterRequest request, int authorId, CancellationToken cancellationToken);
    Task<Result<ChapterResponse>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChapterResponse>> GetByStoryIdAsync(int storyId, CancellationToken cancellationToken);
    Task<Result<ChapterResponse>> UpdateAsync(int id, UpdateChapterRequest request, int authorId, CancellationToken cancellationToken);
    Task<Result<bool>> DeleteAsync(int id, int authorId, CancellationToken cancellationToken);
}
