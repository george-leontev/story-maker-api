namespace StoryMakerApi.Models;

public sealed class Vote
{
    public Guid UserId { get; init; }
    public Guid ChoiceId { get; init; }
    public int SelectedOption { get; init; }

    public User User { get; init; } = null!;
    public Choice Choice { get; init; } = null!;
}
