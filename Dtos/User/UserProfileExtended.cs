namespace StoryMakerApi.Dtos.User;

public sealed record UserProfileExtended(
    ProfileResponse Profile,
    IReadOnlyList<StoryListItem> MyStories);