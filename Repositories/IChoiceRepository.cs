using StoryMakerApi.Models;

namespace StoryMakerApi.Repositories;

public interface IChoiceRepository
{
    Task<Choice?> FindByIdAsync(int id, CancellationToken cancellationToken);
    Task<Choice?> FindByIdForUpdateAsync(int id, CancellationToken cancellationToken);
    Task<bool> HasVotedAsync(int choiceId, int userId, CancellationToken cancellationToken);
    Task RecordVoteAsync(Choice choice, int option, int userId, CancellationToken cancellationToken);
    Task<int> GetVoteCountAsync(int choiceId, int option, CancellationToken cancellationToken);
    Task CloseChoiceAsync(Choice choice, CancellationToken cancellationToken);
}
