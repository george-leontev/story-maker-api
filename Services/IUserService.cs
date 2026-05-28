using StoryMakerApi.Dtos;
using StoryMakerApi.Dtos.User;

namespace StoryMakerApi.Services;

public interface IUserService
{
    Task<ProfileResponse> GetProfileAsync(int userId, CancellationToken cancellationToken);
    Task<UserProfileExtended> GetProfileExtendedAsync(int userId, CancellationToken cancellationToken);
    Task<Result<ProfileResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<Result> UploadAvatarAsync(int userId, IFormFile avatar, CancellationToken cancellationToken);
    Task<Result> DeleteAccountAsync(int userId, CancellationToken cancellationToken);
    Task<PagedResponse<VoteHistoryResponse>> GetVoteHistoryAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<StoryListItem>> GetMyStoriesAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<PagedResponse<VoteHistoryResponse>> GetAuthorVotesAsync(int userId, int page, int pageSize, CancellationToken cancellationToken);
}