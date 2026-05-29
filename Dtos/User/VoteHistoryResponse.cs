namespace StoryMakerApi.Dtos.User;

public sealed record VoteHistoryResponse(
    int ChoiceId,
    int ChapterId,
    int StoryId,
    string StoryTitle,
    string ChapterTitle,
    int SelectedOption,
    string VoteText,
    bool IsClosed,
    int? WinningOption,
    int? Option1Votes,
    int? Option2Votes,
    DateTime VotedAt);