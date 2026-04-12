namespace StoryMakerApi.Models;

public sealed class Story
{
    public int Id { get; init; }
    public string Title { get; internal set; } = string.Empty;
    public string Description { get; internal set; } = string.Empty;
    public int AuthorId { get; init; }
    public float Rating { get; internal set; }
    public DateTime CreatedAt { get; init; }

    public User Author { get; init; } = null!;
    public ICollection<Chapter> Chapters { get; init; } = new List<Chapter>();
    public ICollection<Comment> Comments { get; init; } = new List<Comment>();
    public ICollection<Subscription> Subscriptions { get; init; } = new List<Subscription>();
}
