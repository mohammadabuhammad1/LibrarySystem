using LibrarySystem.Domain.Common;

namespace LibrarySystem.Domain.ValueObjects;

public sealed class Isbn : ValueObject
{
    public string Value { get; }
    public string FormattedValue { get; }

    private Isbn(string value)
    {
        Value = value;
        FormattedValue = FormatIsbn(value);
    }

    public static Isbn Create(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN cannot be null or empty", nameof(isbn));

        string cleanedIsbn = CleanIsbn(isbn);

        if (!IsValidIsbn(cleanedIsbn))
            throw new ArgumentException($"Invalid ISBN format: {isbn}", nameof(isbn));

        return new Isbn(cleanedIsbn);
    }

    public static bool TryCreate(string isbn, out Isbn? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        string cleanedIsbn = CleanIsbn(isbn);

        if (!IsValidIsbn(cleanedIsbn))
            return false;

        result = new Isbn(cleanedIsbn);
        return true;
    }

    private static string CleanIsbn(string isbn)
    {
        return isbn.Replace("-", "", StringComparison.Ordinal)
                   .Replace(" ", "", StringComparison.Ordinal)
                   .Trim();
    }

    private static bool IsValidIsbn(string isbn)
    {
        if (isbn.Length != 10 && isbn.Length != 13)
            return false;

        return isbn.Length == 10 ? IsValidIsbn10(isbn) : IsValidIsbn13(isbn);
    }

    private static bool IsValidIsbn10(string isbn)
    {
        if (isbn.Length != 10)
            return false;

        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            if (!char.IsDigit(isbn[i]))
                return false;

            sum += (isbn[i] - '0') * (10 - i);
        }

        char lastChar = isbn[9];
        if (lastChar == 'X' || lastChar == 'x')
        {
            sum += 10;
        }
        else if (char.IsDigit(lastChar))
        {
            sum += lastChar - '0';
        }
        else
        {
            return false;
        }

        return sum % 11 == 0;
    }

    private static bool IsValidIsbn13(string isbn)
    {
        if (isbn.Length != 13)
            return false;

        if (!isbn.All(char.IsDigit))
            return false;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = isbn[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (isbn[12] - '0');
    }

    private static string FormatIsbn(string isbn)
    {
        return isbn.Length switch
        {
            10 => $"{isbn[..1]}-{isbn[1..5]}-{isbn[5..9]}-{isbn[9..]}",
            13 => $"{isbn[..3]}-{isbn[3..4]}-{isbn[4..8]}-{isbn[8..12]}-{isbn[12..]}",
            _ => isbn
        };
    }

    public override string ToString() => FormattedValue;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(Isbn isbn)
    {
        ArgumentNullException.ThrowIfNull(isbn);
        return isbn.Value;
    }
}