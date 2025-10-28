namespace LibrarySystem.Domain.Queries;

public class QueryResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public T? Data { get; }

    protected QueryResult(bool isSuccess, string error, T? data = default)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
        Data = data;
    }

#pragma warning disable CA1000 // Do not declare static members on generic types
    public static QueryResult<T> Ok(T? data = default)
        => new(true, string.Empty, data);

    public static QueryResult<T> Fail(string error)
        => new(false, error);
#pragma warning restore CA1000 // Do not declare static members on generic types
}