using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Money.Tests;

public sealed class MoneyJsonConverterTests
{
    private static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions().AddBuildingBlocksMoneyConverter();

    [Fact]
    public void Write_emits_string_amount_and_currency()
    {
        var json = JsonSerializer.Serialize(new Money(1234.5m, CurrencyCode.Parse("EUR")), Options);
        json.Should().Be(@"{""amount"":""1234.5000"",""currency"":""EUR""}");
    }

    [Fact]
    public void Read_accepts_string_amount()
    {
        var json = @"{""amount"":""1234.5600"",""currency"":""EUR""}";
        var money = JsonSerializer.Deserialize<Money>(json, Options);
        money.Should().Be(new Money(1234.56m, CurrencyCode.Parse("EUR")));
    }

    [Fact]
    public void Read_accepts_number_amount()
    {
        var json = @"{""amount"":1234.56,""currency"":""EUR""}";
        var money = JsonSerializer.Deserialize<Money>(json, Options);
        money.Should().Be(new Money(1234.56m, CurrencyCode.Parse("EUR")));
    }

    [Fact]
    public void Read_round_trips_through_write()
    {
        var original = new Money(-12.3456m, CurrencyCode.Parse("USD"));
        var json = JsonSerializer.Serialize(original, Options);
        var parsed = JsonSerializer.Deserialize<Money>(json, Options);
        parsed.Should().Be(original);
    }

    [Fact]
    public void Read_skips_unknown_properties()
    {
        var json = @"{""extra"":42,""amount"":""1.0000"",""currency"":""EUR""}";
        var money = JsonSerializer.Deserialize<Money>(json, Options);
        money.Should().Be(new Money(1m, CurrencyCode.Parse("EUR")));
    }

    [Fact]
    public void Read_rejects_missing_amount()
    {
        var json = @"{""currency"":""EUR""}";
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_missing_currency()
    {
        var json = @"{""amount"":""1.0000""}";
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_null_amount()
    {
        var json = @"{""amount"":null,""currency"":""EUR""}";
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_empty_string_amount()
    {
        // Exercises the string branch of the amount parser: a present-but-empty value must fail
        // (decimal.Parse on an empty span throws FormatException, which the converter wraps).
        var json = @"{""amount"":"""",""currency"":""EUR""}";
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_null_currency()
    {
        var json = @"{""amount"":""1.0000"",""currency"":null}";
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Theory]
    [InlineData(@"{""amount"":true,""currency"":""EUR""}")]
    [InlineData(@"{""amount"":false,""currency"":""EUR""}")]
    [InlineData(@"{""amount"":{},""currency"":""EUR""}")]
    [InlineData(@"{""amount"":[],""currency"":""EUR""}")]
    public void Read_rejects_non_string_non_number_amount(string json)
    {
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_non_string_currency()
    {
        var json = @"{""amount"":""1.0000"",""currency"":42}";
        Action act = () => JsonSerializer.Deserialize<Money>(json, Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_non_object_root()
    {
        Action act = () => JsonSerializer.Deserialize<Money>("[]", Options);
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Read_rejects_non_property_token_inside_object()
    {
        // Drive the converter through a Utf8JsonReader with comments allowed so the reader surfaces
        // a Comment token inside the object, exercising the "not a property name" branch.
        Assert.Throws<JsonException>(() => ReadWithComments(
            "{/* hi */\"amount\":\"1.0000\",\"currency\":\"EUR\"}"u8));
    }

    private static Money ReadWithComments(ReadOnlySpan<byte> json)
    {
        var converter = new MoneyJsonConverter();
        var readerOpts = new JsonReaderOptions { CommentHandling = JsonCommentHandling.Allow };
        var reader = new Utf8JsonReader(json, readerOpts);
        reader.Read();
        return converter.Read(ref reader, typeof(Money), Options);
    }

    [Fact]
    public void AddBuildingBlocksMoneyConverter_returns_same_options_instance()
    {
        var opts = new JsonSerializerOptions();
        var returned = opts.AddBuildingBlocksMoneyConverter();
        returned.Should().BeSameAs(opts);
        returned.Converters.Should().ContainSingle(c => c is MoneyJsonConverter);
    }
}
