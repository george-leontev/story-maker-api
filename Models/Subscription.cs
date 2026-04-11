namespace StoryMakerApi.Models;

public sealed class Subscription
{
    public Guid UserId { get; init; }
    public Guid StoryId { get; init; }

    public User User { get; init; } = null!;
    public Story Story { get; init; } = null!;
}
