using FsCheck;
using FsCheck.Fluent;

namespace GetDue.BuildingBlocks.Money.Tests;

/// <summary>FsCheck arbitraries for <see cref="Money"/> and its constituents.</summary>
public static class MoneyGenerators
{
    private static readonly string[] Currencies = ["EUR", "USD", "GBP", "PLN", "JPY"];

    public static Arbitrary<CurrencyCode> CurrencyArb() =>
        Gen.Elements(Currencies).Select(CurrencyCode.Parse).ToArbitrary();

    public static Arbitrary<decimal> MoneyAmountArb()
    {
        // Scale <= 4, magnitude < 10^14 — leaves ample room for additive ops in range.
        // Compose a long from two ints (Fluent.Gen.Choose only takes int32 bounds).
        var high = Gen.Choose(-99_999_999, 99_999_999);    // up to 10^8
        var low = Gen.Choose(0, 999_999);                  // up to 10^6
        return high.Zip(low).Select(t =>
        {
            long combined = (long)t.Item1 * 1_000_000L + (t.Item1 < 0 ? -t.Item2 : t.Item2);
            return (decimal)combined / 10_000m;
        }).ToArbitrary();
    }

    public static Arbitrary<Money> MoneyArb()
    {
        var amount = MoneyAmountArb().Generator;
        var currency = CurrencyArb().Generator;
        return amount.Zip(currency).Select(t => new Money(t.Item1, t.Item2)).ToArbitrary();
    }
}
