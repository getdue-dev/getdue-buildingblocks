using System.Text.RegularExpressions;

namespace GetDue.BuildingBlocks.Money;

/// <summary>ISO-4217 three-letter currency code, normalized to upper-case.</summary>
public readonly partial record struct CurrencyCode
{
    [GeneratedRegex("^[A-Z]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex Shape();

    /// <summary>The three uppercase A-Z letters.</summary>
    public string Value { get; }

    private CurrencyCode(string value)
    {
        Value = value;
    }

    /// <summary>Parses and upper-cases <paramref name="code"/>.</summary>
    /// <exception cref="ArgumentException">When <paramref name="code"/> is null or empty.</exception>
    /// <exception cref="FormatException">When <paramref name="code"/> is not three A-Z letters.</exception>
    public static CurrencyCode Parse(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            throw new ArgumentException("Currency code must not be null or empty.", nameof(code));
        }

        var upper = code.ToUpperInvariant();
        if (!Shape().IsMatch(upper))
        {
            throw new FormatException("Currency code must be exactly three A-Z letters.");
        }

        return new CurrencyCode(upper);
    }

    /// <summary>Attempts to parse <paramref name="code"/>; never throws.</summary>
    public static bool TryParse(string? code, out CurrencyCode result)
    {
        if (string.IsNullOrEmpty(code))
        {
            result = default;
            return false;
        }

        var upper = code.ToUpperInvariant();
        if (!Shape().IsMatch(upper))
        {
            result = default;
            return false;
        }

        result = new CurrencyCode(upper);
        return true;
    }

    /// <inheritdoc/>
    public override string ToString() => Value ?? string.Empty;
}
