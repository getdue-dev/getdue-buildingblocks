using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Money.Tests;

public sealed class MoneyTests
{
    private static readonly CurrencyCode Eur = CurrencyCode.Parse("EUR");
    private static readonly CurrencyCode Usd = CurrencyCode.Parse("USD");

    [Theory]
    [InlineData("1.23445", "1.2344")]   // half-even → down (4 is even)
    [InlineData("1.23455", "1.2346")]   // half-even → up (5 → 6 even)
    [InlineData("-1.23455", "-1.2346")]
    [InlineData("1.23435", "1.2344")]   // half-even → 4 (even)
    [InlineData("1.00005", "1.0000")]   // half-even → 0 (even)
    [InlineData("1.00015", "1.0002")]   // half-even → 2 (even)
    public void Constructor_rounds_to_scale_four_banker_style(string input, string expected)
    {
        var amount = decimal.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        var expectedAmount = decimal.Parse(expected, System.Globalization.CultureInfo.InvariantCulture);

        new Money(amount, Eur).Amount.Should().Be(expectedAmount);
    }

    [Fact]
    public void DivideBy_zero_throws()
    {
        var m = new Money(1m, Eur);
        Action act = () => m.DivideBy(0m);
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void DivideBy_zero_via_operator_throws()
    {
        var m = new Money(1m, Eur);
        Action act = () => { var _ = m / 0m; };
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Constructor_accepts_magnitude_ceiling()
    {
        var ceiling = 9_999_999_999_999.9999m;
        var m = new Money(ceiling, Eur);
        m.Amount.Should().Be(ceiling);
    }

    [Fact]
    public void Constructor_rejects_amount_at_or_above_range()
    {
        Action act = () => _ = new Money(10_000_000_000_000_000m, Eur);
        act.Should().Throw<OverflowException>();
    }

    [Fact]
    public void Multiply_that_overflows_throws()
    {
        var m = new Money(9_999_999_999_999.9999m, Eur);
        Action act = () => _ = m * 1_000_000m;
        act.Should().Throw<OverflowException>();
    }

    [Fact]
    public void ToString_uses_invariant_culture_with_four_fractional_digits()
    {
        new Money(1.5m, Eur).ToString().Should().Be("1.5000 EUR");
    }

    [Fact]
    public void ToString_keeps_no_thousand_separators()
    {
        new Money(1_234_567.89m, Eur).ToString().Should().Be("1234567.8900 EUR");
    }

    [Fact]
    public void CurrencyMismatchException_exposes_codes_without_amounts()
    {
        var a = new Money(1234.56m, Eur);
        var b = new Money(78.90m, Usd);

        Action act = () => _ = a + b;
        var ex = act.Should().Throw<CurrencyMismatchException>().Which;

        ex.LeftCurrency.Should().Be("EUR");
        ex.RightCurrency.Should().Be("USD");
        ex.Message.Should().Be("Cannot operate on Money with different currencies: EUR vs USD.");
        ex.Message.Should().NotContain("1234");
        ex.Message.Should().NotContain("78");
    }

    [Fact]
    public void Zero_is_zero_positive_negative_flags()
    {
        var zero = Money.Zero(Eur);
        zero.IsZero.Should().BeTrue();
        zero.IsPositive.Should().BeFalse();
        zero.IsNegative.Should().BeFalse();
    }

    [Fact]
    public void Positive_amount_flags()
    {
        var m = new Money(1m, Eur);
        m.IsPositive.Should().BeTrue();
        m.IsNegative.Should().BeFalse();
        m.IsZero.Should().BeFalse();
    }

    [Fact]
    public void Negative_amount_flags()
    {
        var m = new Money(-1m, Eur);
        m.IsNegative.Should().BeTrue();
        m.IsPositive.Should().BeFalse();
        m.IsZero.Should().BeFalse();
    }

    [Fact]
    public void Subtract_same_currency_works()
    {
        var a = new Money(10m, Eur);
        var b = new Money(4m, Eur);
        (a - b).Should().Be(new Money(6m, Eur));
        a.Subtract(b).Should().Be(new Money(6m, Eur));
    }

    [Fact]
    public void Add_same_currency_works()
    {
        var a = new Money(10m, Eur);
        var b = new Money(4m, Eur);
        (a + b).Should().Be(new Money(14m, Eur));
        a.Add(b).Should().Be(new Money(14m, Eur));
    }

    [Fact]
    public void Subtract_different_currencies_throws()
    {
        var a = new Money(10m, Eur);
        var b = new Money(4m, Usd);
        Action act = () => _ = a - b;
        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Multiply_by_scalar_via_lhs_and_rhs_operators_match()
    {
        var m = new Money(2m, Eur);
        (m * 3m).Should().Be(new Money(6m, Eur));
        (3m * m).Should().Be(new Money(6m, Eur));
        m.Multiply(3m).Should().Be(new Money(6m, Eur));
    }

    [Fact]
    public void Divide_by_scalar_works()
    {
        var m = new Money(6m, Eur);
        (m / 2m).Should().Be(new Money(3m, Eur));
        m.DivideBy(2m).Should().Be(new Money(3m, Eur));
    }

    [Fact]
    public void Comparison_operators_within_same_currency()
    {
        var a = new Money(1m, Eur);
        var b = new Money(2m, Eur);
        var aCopy = new Money(1m, Eur);

        (a < b).Should().BeTrue();
        (b > a).Should().BeTrue();
        (a <= b).Should().BeTrue();
        (b >= a).Should().BeTrue();
        (a <= aCopy).Should().BeTrue();
        (a >= aCopy).Should().BeTrue();
        (a < aCopy).Should().BeFalse();
        (a > aCopy).Should().BeFalse();
    }

    [Fact]
    public void CompareTo_different_currencies_throws()
    {
        var a = new Money(1m, Eur);
        var b = new Money(1m, Usd);

        Action act = () => a.CompareTo(b);
        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Negate_uses_operator_and_method()
    {
        var m = new Money(5m, Eur);
        (-m).Should().Be(new Money(-5m, Eur));
        m.Negate().Should().Be(new Money(-5m, Eur));
    }
}
