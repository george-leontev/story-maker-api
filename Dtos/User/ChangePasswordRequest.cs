namespace StoryMakerApi.Dtos.User;

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);