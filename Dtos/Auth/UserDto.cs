namespace StoryMakerApi.Dtos.Auth;

public sealed record UserDto(int Id, string Username, string Email, DateTime CreatedAt);
