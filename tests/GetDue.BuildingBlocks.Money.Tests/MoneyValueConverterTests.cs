using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetDue.BuildingBlocks.Money.Tests;

public sealed class MoneyValueConverterTests
{
    [Fact]
    public void Converts_money_to_canonical_string()
    {
        var converter = new MoneyValueConverter();
        var money = new Money(1234.5m, CurrencyCode.Parse("EUR"));

        var stored = (string)converter.ConvertToProvider(money)!;
        stored.Should().Be("1234.5000 EUR");
    }

    [Fact]
    public void Round_trips_through_provider_and_model()
    {
        var converter = new MoneyValueConverter();
        var original = new Money(-9876.5432m, CurrencyCode.Parse("USD"));

        var stored = converter.ConvertToProvider(original);
        var roundTripped = (Money)converter.ConvertFromProvider(stored)!;

        roundTripped.Should().Be(original);
    }

    [Theory]
    [InlineData("noseparator")]
    [InlineData(" EUR")]
    [InlineData("1.0000 ")]
    public void Parsing_invalid_strings_throws(string bad)
    {
        var converter = new MoneyValueConverter();
        Action act = () => _ = converter.ConvertFromProvider(bad);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void AsMoney_returns_same_builder_for_chaining()
    {
        var modelBuilder = new ModelBuilder();
        var entity = modelBuilder.Entity<MoneyHolder>();
        entity.HasKey(h => h.Id);
        var property = entity.Property(h => h.Price);

        var result = property.AsMoney();

        result.Should().BeSameAs(property);
        property.Metadata.GetMaxLength().Should().Be(24);
        property.Metadata.GetValueConverter().Should().BeOfType<MoneyValueConverter>();
    }

    private sealed class MoneyHolder
    {
        public int Id { get; set; }
        public Money Price { get; set; }
    }
}
