using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    
    Task UpdateAsync(User user, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> CollectUploadedFilesForUserAsync(int id, CancellationToken cancellationToken);
    Task DeleteWithCascadeAsync(int id, CancellationToken cancellationToken);
    Task<(int StoriesCount, int VotesCount, int CommentsCount)> GetUserStatsAsync(int userId, CancellationToken cancellationToken);
    Task<User?> FindByIdWithDetailsAsync(int id, CancellationToken cancellationToken);
}
