namespace StoryMakerApi.Models;

public sealed class Story
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public Guid AuthorId { get; init; }
    public float Rating { get; set; }
    public DateTime CreatedAt { get; init; }

    public User Author { get; init; } = null!;
    public ICollection<Chapter> Chapters { get; init; } = new List<Chapter>();
    public ICollection<Comment> Comments { get; init; } = new List<Comment>();
    public ICollection<Subscription> Subscriptions { get; init; } = new List<Subscription>();
}
