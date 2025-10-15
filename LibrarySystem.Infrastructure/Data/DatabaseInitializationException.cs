using System;

namespace LibrarySystem.Infrastructure.Data;

public class DatabaseInitializationException : Exception
{
    public DatabaseInitializationException()
    {
    }

    public DatabaseInitializationException(string message)
        : base(message)
    {
    }

    public DatabaseInitializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}