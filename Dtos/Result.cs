namespace StoryMakerApi.Dtos;

public sealed record Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
    }

    private Result() => IsSuccess = true;

    public static Result Success() => new();
    public static Result Failure(string error) => new(error);
}

public sealed record Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
}
