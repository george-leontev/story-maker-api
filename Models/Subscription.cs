namespace StoryMakerApi.Models;

public sealed class Subscription
{
    public int UserId { get; init; }
    public int StoryId { get; init; }

    public User User { get; init; } = null!;
    public Story Story { get; init; } = null!;
}
