namespace StoryMakerApi.Dtos.Auth;

public sealed record RegisterRequest(string Username, string Email, string Password);
