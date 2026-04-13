namespace StoryMakerApi.Models;

public sealed class User
{
    public int Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string PasswordHash { get; init; }
    public DateTime CreatedAt { get; init; }

    public ICollection<Story> Stories { get; init; } = new List<Story>();
    public ICollection<Vote> Votes { get; init; } = new List<Vote>();
    public ICollection<Comment> Comments { get; init; } = new List<Comment>();
    public ICollection<Subscription> Subscriptions { get; init; } = new List<Subscription>();
    public ICollection<StoryRating> Ratings { get; init; } = new List<StoryRating>();
}
