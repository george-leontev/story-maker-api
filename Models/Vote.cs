namespace StoryMakerApi.Models;

public sealed class Vote
{
    public int UserId { get; init; }
    public int ChoiceId { get; init; }
    public int SelectedOption { get; init; }

    public User User { get; init; } = null!;
    public Choice Choice { get; init; } = null!;
}
