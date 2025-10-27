namespace LibrarySystem.Domain.Commands;

public class CommandResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public object? Value { get; }

    protected CommandResult(bool isSuccess, string error, object? value = null)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public static CommandResult Ok(object? value = null)
        => new CommandResult(true, string.Empty, value);

    public static CommandResult Fail(string error)
        => new CommandResult(false, error);
}