using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GetDue.BuildingBlocks.Money;

/// <summary>
/// EF Core value converter that stores <see cref="Money"/> as a single <c>varchar(24)</c> column
/// formatted <c>"1234.5600 EUR"</c>.
/// </summary>
/// <remarks>
/// A two-column split (amount + currency) is the ideal physical model for analytics, but EF expresses it
/// through complex-type / owned mapping at the <c>ModelBuilder</c> level, not via a <see cref="ValueConverter{TModel, TProvider}"/>.
/// We pick the single-column form here because it stays self-contained (no <c>DbContext</c> needed to test),
/// keeps round-trips byte-exact, and is enough for the Phase-0 building-block layer. Aggregating services
/// that need a split column should configure two scalar properties on the entity instead.
/// </remarks>
public sealed class MoneyValueConverter : ValueConverter<Money, string>
{
    /// <summary>Creates a new <see cref="MoneyValueConverter"/>.</summary>
    public MoneyValueConverter()
        : base(
            m => m.ToString(),
            s => Parse(s))
    {
    }

    private static Money Parse(string text)
    {
        var space = text.IndexOf(' ');
        if (space <= 0 || space == text.Length - 1)
        {
            throw new FormatException("Money string must be \"<amount> <currency>\".");
        }

        var amount = decimal.Parse(
            text.AsSpan(0, space),
            NumberStyles.Number,
            CultureInfo.InvariantCulture);
        var currency = CurrencyCode.Parse(text[(space + 1)..]);
        return new Money(amount, currency);
    }
}

/// <summary>EF Core helpers for mapping <see cref="Money"/> properties.</summary>
public static class MoneyEfExtensions
{
    /// <summary>
    /// Configures the property to persist via <see cref="MoneyValueConverter"/> with a 24-char max length.
    /// Provider-specific column type (e.g. <c>varchar(24)</c>) should be set by the caller's relational config.
    /// </summary>
    public static PropertyBuilder<Money> AsMoney(this PropertyBuilder<Money> b)
    {
        b.HasConversion(new MoneyValueConverter());
        b.HasMaxLength(24);
        return b;
    }
}
