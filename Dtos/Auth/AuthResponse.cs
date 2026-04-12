namespace StoryMakerApi.Dtos.Auth;

public sealed record AuthResponse(string Token, DateTime ExpiresAt);
