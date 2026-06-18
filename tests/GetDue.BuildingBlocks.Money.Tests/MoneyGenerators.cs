using FsCheck;
using FsCheck.Fluent;

namespace GetDue.BuildingBlocks.Money.Tests;

/// <summary>
/// FsCheck arbitraries for <see cref="Money"/> and its constituents. Amounts span
/// <c>[-10^14, 10^14)</c> at scale 4, so additive and multiplicative property tests can exercise
/// values close to the <c>numeric(19,4)</c> boundary without saturating it.
/// </summary>
public static class MoneyGenerators
{
    private static readonly string[] Currencies = ["EUR", "USD", "GBP", "PLN", "JPY"];

    public static Arbitrary<CurrencyCode> CurrencyArb() =>
        Gen.Elements(Currencies).Select(CurrencyCode.Parse).ToArbitrary();

    public static Arbitrary<decimal> MoneyAmountArb()
    {
        // Compose a signed long integer part in [-10^14, 10^14) from three int32 chunks
        // (Gen.Choose only takes int32 bounds). Scale is fixed at 4 via the fractional [0, 10000).
        var sign = Gen.Elements(-1, 1);
        var high = Gen.Choose(0, 99_999);          // 0..10^5 - 1
        var mid = Gen.Choose(0, 999_999_999);      // 0..10^9 - 1
        var frac = Gen.Choose(0, 9_999);           // 0..10^4 - 1

        return sign.Zip(high).Zip(mid).Zip(frac).Select(t =>
        {
            var (((s, h), m), f) = t;
            long integerPart = (long)h * 1_000_000_000L + m;          // [0, 10^14)
            decimal magnitude = (decimal)integerPart + (decimal)f / 10_000m;
            return s * magnitude;
        }).ToArbitrary();
    }

    public static Arbitrary<Money> MoneyArb()
    {
        var amount = MoneyAmountArb().Generator;
        var currency = CurrencyArb().Generator;
        return amount.Zip(currency).Select(t => new Money(t.Item1, t.Item2)).ToArbitrary();
    }
}
