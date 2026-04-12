namespace StoryMakerApi.Dtos.Choice;

// Author view — always shows live vote counts
public sealed record ChoiceAuthorResponse(
    int Id,
    int ChapterId,
    string Option1Text,
    string Option2Text,
    int Option1Votes,
    int Option2Votes,
    DateTime ExpiresAt,
    bool IsClosed
);
