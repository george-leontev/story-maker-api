namespace StoryMakerApi.Dtos.Choice;

public sealed record CreateChoiceRequest(string Option1Text, string Option2Text, int DurationInMinutes);
