using System.Globalization;

namespace GetDue.BuildingBlocks.Money;

/// <summary>
/// A monetary amount paired with its currency. Immutable, scale-4 decimal, never floats.
/// </summary>
/// <remarks>
/// <c>default(Money)</c> is not a valid value — it has a <c>default(CurrencyCode)</c> whose
/// <see cref="CurrencyCode.Value"/> is <c>null</c>, which breaks serialization and EF round-trips.
/// Consumers should construct via <see cref="Money(decimal, CurrencyCode)"/> or
/// <see cref="Zero(CurrencyCode)"/>.
/// </remarks>
public readonly record struct Money : IComparable<Money>
{
    // 'static readonly' (not const) so coverlet's instrumentation observes the field-load branch.
    private static readonly decimal RangeCeiling = 1_000_000_000_000_000m; // 10^15

    /// <summary>The signed amount, always at decimal scale 4 (banker's rounding).</summary>
    public decimal Amount { get; }

    /// <summary>The ISO-4217 currency code.</summary>
    public CurrencyCode Currency { get; }

    /// <summary>Creates a new <see cref="Money"/>, rounding the amount to scale 4 (ToEven).</summary>
    /// <exception cref="ArgumentException">When <paramref name="currency"/> is <c>default(CurrencyCode)</c>.</exception>
    /// <exception cref="OverflowException">When the rounded amount falls outside <c>numeric(19,4)</c>.</exception>
    public Money(decimal amount, CurrencyCode currency)
    {
        if (currency.Value is null)
        {
            throw new ArgumentException("Currency must be specified.", nameof(currency));
        }

        var rounded = RoundToScale4(amount);
        EnsureInRange(rounded);
        Amount = rounded;
        Currency = currency;
    }

    /// <summary>Returns a zero-valued <see cref="Money"/> in <paramref name="currency"/>.</summary>
    public static Money Zero(CurrencyCode currency) => new(0m, currency);

    /// <summary>Returns <c>true</c> when the amount is exactly zero.</summary>
    public bool IsZero => Amount == 0m;

    /// <summary>Returns <c>true</c> when the amount is strictly positive.</summary>
    public bool IsPositive => Amount > 0m;

    /// <summary>Returns <c>true</c> when the amount is strictly negative.</summary>
    public bool IsNegative => Amount < 0m;

    /// <summary>Adds two same-currency <see cref="Money"/> values.</summary>
    /// <exception cref="CurrencyMismatchException">When currencies differ.</exception>
    public Money Add(Money other)
    {
        EnsureSameCurrency(this, other);
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>Subtracts a same-currency <see cref="Money"/> value.</summary>
    /// <exception cref="CurrencyMismatchException">When currencies differ.</exception>
    public Money Subtract(Money other)
    {
        EnsureSameCurrency(this, other);
        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>Returns the additive inverse.</summary>
    public Money Negate() => new(-Amount, Currency);

    /// <summary>Scales the amount by <paramref name="factor"/> and rebanks to scale 4.</summary>
    /// <exception cref="OverflowException">When the result falls outside <c>numeric(19,4)</c>.</exception>
    public Money Multiply(decimal factor) => new(Amount * factor, Currency);

    /// <summary>Divides the amount by <paramref name="divisor"/> and rebanks to scale 4.</summary>
    /// <exception cref="DivideByZeroException">When <paramref name="divisor"/> is zero.</exception>
    /// <exception cref="OverflowException">When the result falls outside <c>numeric(19,4)</c>.</exception>
    public Money DivideBy(decimal divisor)
    {
        if (divisor == 0m)
        {
            throw new DivideByZeroException();
        }

        return new Money(Amount / divisor, Currency);
    }

    /// <summary>Adds two same-currency <see cref="Money"/> values.</summary>
    public static Money operator +(Money a, Money b) => a.Add(b);

    /// <summary>Subtracts <paramref name="b"/> from <paramref name="a"/>.</summary>
    public static Money operator -(Money a, Money b) => a.Subtract(b);

    /// <summary>Returns the additive inverse.</summary>
    public static Money operator -(Money m) => m.Negate();

    /// <summary>Scales <paramref name="m"/> by <paramref name="f"/>.</summary>
    public static Money operator *(Money m, decimal f) => m.Multiply(f);

    /// <summary>Scales <paramref name="m"/> by <paramref name="f"/>.</summary>
    public static Money operator *(decimal f, Money m) => m.Multiply(f);

    /// <summary>Divides <paramref name="m"/> by <paramref name="d"/>.</summary>
    public static Money operator /(Money m, decimal d) => m.DivideBy(d);

    /// <summary>Orders two same-currency <see cref="Money"/> values.</summary>
    /// <exception cref="CurrencyMismatchException">When currencies differ.</exception>
    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    /// <summary>Returns <c>true</c> when <paramref name="a"/> is strictly less than <paramref name="b"/>.</summary>
    public static bool operator <(Money a, Money b) => a.CompareTo(b) < 0;

    /// <summary>Returns <c>true</c> when <paramref name="a"/> is strictly greater than <paramref name="b"/>.</summary>
    public static bool operator >(Money a, Money b) => a.CompareTo(b) > 0;

    /// <summary>Returns <c>true</c> when <paramref name="a"/> is less than or equal to <paramref name="b"/>.</summary>
    public static bool operator <=(Money a, Money b) => a.CompareTo(b) <= 0;

    /// <summary>Returns <c>true</c> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>.</summary>
    public static bool operator >=(Money a, Money b) => a.CompareTo(b) >= 0;

    /// <summary>Canonical representation: <c>"1234.5600 EUR"</c> (InvariantCulture, no thousand separators).</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Amount:0.0000} {Currency.Value}");

    private static decimal RoundToScale4(decimal value) =>
        decimal.Round(value, 4, MidpointRounding.ToEven);

    private static void EnsureInRange(decimal value)
    {
        if (Math.Abs(value) >= RangeCeiling)
        {
            throw new OverflowException("Money amount falls outside the numeric(19,4) range.");
        }
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
        {
            throw new CurrencyMismatchException(left.Currency.Value, right.Currency.Value);
        }
    }
}
