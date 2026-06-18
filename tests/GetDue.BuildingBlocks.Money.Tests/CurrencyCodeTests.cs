using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Money.Tests;

public sealed class CurrencyCodeTests
{
    [Fact]
    public void Parse_uppercases_input()
    {
        CurrencyCode.Parse("eur").Should().Be(CurrencyCode.Parse("EUR"));
    }

    [Fact]
    public void Parse_keeps_uppercase_when_already_uppercase()
    {
        CurrencyCode.Parse("USD").Value.Should().Be("USD");
    }

    [Theory]
    [InlineData("eu")]
    [InlineData("eurs")]
    [InlineData("e1r")]
    [InlineData("123")]
    [InlineData(" eur")]
    public void Parse_rejects_non_three_letter_input(string bad)
    {
        Action act = () => _ = CurrencyCode.Parse(bad);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void Parse_rejects_null()
    {
        Action act = () => _ = CurrencyCode.Parse(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Parse_rejects_empty()
    {
        Action act = () => _ = CurrencyCode.Parse(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("eur", true, "EUR")]
    [InlineData("USD", true, "USD")]
    [InlineData("eu", false, "")]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    public void TryParse_returns_expected(string? input, bool expected, string expectedValue)
    {
        CurrencyCode.TryParse(input, out var result).Should().Be(expected);
        if (expected)
        {
            result.Value.Should().Be(expectedValue);
        }
    }

    [Fact]
    public void ToString_returns_value()
    {
        CurrencyCode.Parse("PLN").ToString().Should().Be("PLN");
    }

    [Fact]
    public void Default_struct_ToString_is_empty()
    {
        default(CurrencyCode).ToString().Should().Be(string.Empty);
    }
}
