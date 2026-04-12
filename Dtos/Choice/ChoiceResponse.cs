namespace StoryMakerApi.Dtos.Choice;

// Reader view — vote counts hidden unless choice is closed
public sealed record ChoiceResponse(
    int Id,
    int ChapterId,
    string Option1Text,
    string Option2Text,
    DateTime ExpiresAt,
    bool IsClosed,
    int? Option1Votes,      // null while active, revealed when closed
    int? Option2Votes     // null while active, revealed when closed
    );
