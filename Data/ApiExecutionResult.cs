namespace storage.Data;

public sealed class ApiExecutionResult
{
    public bool Success { get; init; }
    public object? Data { get; init; }
    public string? Exception { get; init; }
    public Dictionary<string, string>? Errors { get; init; }

    public static ApiExecutionResult FromException(string exception)
    {
        return new ApiExecutionResult
        {
            Success = false,
            Exception = exception
        };
    }

    public static ApiExecutionResult SuccessData(object data)
    {
        return new ApiExecutionResult
        {
            Success = true,
            Data = data
        };
    }

    public static ApiExecutionResult FromErrors(Dictionary<string, string> errors)
    {
        return new ApiExecutionResult
        {
            Success = false,
            Errors = errors
        };
    }
}