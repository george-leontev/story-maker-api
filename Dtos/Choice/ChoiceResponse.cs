namespace StoryMakerApi.Dtos.Choice;

// Reader view — vote counts hidden unless choice is closed
public sealed record ChoiceResponse(
    int Id,
    int ChapterId,
    string Option1Text,
    string Option2Text,
    DateTime ExpiresAt,
    bool IsClosed,
    int? WinningOption,      // 1, 2, or 0 (tie). Null while active.
    int? Option1Votes,       // null while active, revealed when closed
    int? Option2Votes        // null while active, revealed when closed
);
