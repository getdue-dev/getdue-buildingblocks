using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;

namespace GetDue.BuildingBlocks.Money.Tests;

[Properties(Arbitrary = [typeof(MoneyGenerators)], MaxTest = 200)]
public sealed class MoneyPropertyTests
{
    [Property]
    public void Add_is_associative_within_same_currency(decimal a, decimal b, decimal c, CurrencyCode cc)
    {
        var ma = SafeMoney(a, cc);
        var mb = SafeMoney(b, cc);
        var mc = SafeMoney(c, cc);

        ((ma + mb) + mc).Should().Be(ma + (mb + mc));
    }

    [Property]
    public void Add_is_commutative_within_same_currency(decimal a, decimal b, CurrencyCode cc)
    {
        var ma = SafeMoney(a, cc);
        var mb = SafeMoney(b, cc);

        (ma + mb).Should().Be(mb + ma);
    }

    [Property]
    public void Zero_is_additive_identity(Money m)
    {
        (m + Money.Zero(m.Currency)).Should().Be(m);
        (Money.Zero(m.Currency) + m).Should().Be(m);
    }

    [Property]
    public void Negation_is_additive_inverse(Money m)
    {
        (m + (-m)).Should().Be(Money.Zero(m.Currency));
    }

    [Property]
    public void Different_currencies_throw_and_never_leak_amounts(decimal a, decimal b)
    {
        var ma = SafeMoney(a, CurrencyCode.Parse("EUR"));
        var mb = SafeMoney(b, CurrencyCode.Parse("USD"));

        AssertCurrencyMismatchClean(() => { var _ = ma + mb; }, ma, mb);
        AssertCurrencyMismatchClean(() => { var _ = ma - mb; }, ma, mb);
        AssertCurrencyMismatchClean(() => ma.CompareTo(mb), ma, mb);
        AssertCurrencyMismatchClean(() => { var _ = ma < mb; }, ma, mb);
        AssertCurrencyMismatchClean(() => { var _ = ma > mb; }, ma, mb);
        AssertCurrencyMismatchClean(() => { var _ = ma <= mb; }, ma, mb);
        AssertCurrencyMismatchClean(() => { var _ = ma >= mb; }, ma, mb);
    }

    [Property]
    public void Constructed_amount_has_scale_at_most_four(decimal a, CurrencyCode cc)
    {
        var m = SafeMoney(a, cc);
        Scale(m.Amount).Should().BeLessThanOrEqualTo(4);
    }

    [Property]
    public void CompareTo_is_anti_symmetric_and_reflexive(decimal a, decimal b, CurrencyCode cc)
    {
        var ma = SafeMoney(a, cc);
        var mb = SafeMoney(b, cc);

        (ma < mb).Should().Be(mb > ma);
        (ma <= mb).Should().Be(mb >= ma);
        ma.CompareTo(ma).Should().Be(0);
    }

    [Property]
    public void Multiply_by_zero_yields_zero(Money m)
    {
        (m * 0m).Should().Be(Money.Zero(m.Currency));
    }

    [Property]
    public void Division_round_trip_is_within_one_ulp_at_scale_four(Money m, decimal k)
    {
        if (k == 0m || !InRange(m.Amount * k))
        {
            return;
        }

        var roundtrip = (m * k) / k;
        Math.Abs(roundtrip.Amount - m.Amount).Should().BeLessThanOrEqualTo(0.0001m);
    }

    [Property]
    public void Equals_implies_equal_hashcode(decimal a, CurrencyCode cc)
    {
        var ma = SafeMoney(a, cc);
        var mb = SafeMoney(a, cc);

        ma.Should().Be(mb);
        ma.GetHashCode().Should().Be(mb.GetHashCode());
    }

    [Property]
    public void Negation_is_involutive(Money m)
    {
        (-(-m)).Should().Be(m);
    }

    [Property]
    public void Multiplication_distributes_over_addition_within_tolerance(
        decimal a, decimal b, decimal k, CurrencyCode cc)
    {
        var withinRange = InRange(a) && InRange(b) && InRange(a + b)
            && InRange(a * k) && InRange(b * k) && InRange((a + b) * k);
        if (!withinRange)
        {
            return;
        }

        var ma = SafeMoney(a, cc);
        var mb = SafeMoney(b, cc);

        var left = (ma + mb) * k;
        var right = (ma * k) + (mb * k);
        Math.Abs(left.Amount - right.Amount).Should().BeLessThanOrEqualTo(0.0001m);
    }

    private static Money SafeMoney(decimal a, CurrencyCode cc)
    {
        var clamped = decimal.Round(a, 4, MidpointRounding.ToEven);
        return new Money(clamped, cc);
    }

    private static bool InRange(decimal v) => Math.Abs(v) < 1_000_000_000_000_000m;

    private static int Scale(decimal value)
    {
        // The exponent (scale) is byte 3, lower 16 bits, of decimal.GetBits.
        Span<int> bits = stackalloc int[4];
        _ = decimal.GetBits(value, bits);
        return (bits[3] >> 16) & 0x7F;
    }

    private static void AssertCurrencyMismatchClean(Action op, Money left, Money right)
    {
        var ex = op.Should().Throw<CurrencyMismatchException>().Which;
        ex.Message.Should().NotContain(left.Amount.ToString());
        ex.Message.Should().NotContain(right.Amount.ToString());
        ex.LeftCurrency.Should().Be(left.Currency.Value);
        ex.RightCurrency.Should().Be(right.Currency.Value);
    }
}
