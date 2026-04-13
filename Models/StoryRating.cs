namespace StoryMakerApi.Models;

public sealed class StoryRating
{
    public int Id { get; init; }
    public int StoryId { get; init; }
    public int UserId { get; init; }
    public int Score { get; set; }       // 1-5
    public DateTime CreatedAt { get; init; }

    public Story Story { get; init; } = null!;
    public User User { get; init; } = null!;
}
