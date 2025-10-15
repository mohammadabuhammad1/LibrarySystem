namespace LibrarySystem.API.Errors;

public class ApiError(int statusCode, string? message = null, string? details = null) : ApiResponse(statusCode, message)
{
    public string? Details { get; set; } = details;
}

/*
* after this class, we'll create some middleware so that we can handle exceptions and use ApiError Class,
* In event that we get an exception
*/