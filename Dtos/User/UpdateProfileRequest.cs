namespace StoryMakerApi.Dtos.User;

public sealed record UpdateProfileRequest(
    string? Username,
    string? Email);