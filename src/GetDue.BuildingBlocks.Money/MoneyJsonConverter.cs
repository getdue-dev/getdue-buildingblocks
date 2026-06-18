using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GetDue.BuildingBlocks.Money;

/// <summary><see cref="JsonConverter{T}"/> for <see cref="Money"/>: <c>{ "amount": "1234.5600", "currency": "EUR" }</c>.</summary>
/// <remarks>
/// Amounts are serialized as a string to avoid JavaScript float precision loss. The reader is tolerant —
/// it accepts both string and number tokens for <c>amount</c> — but rejects missing or null members.
/// </remarks>
public sealed class MoneyJsonConverter : JsonConverter<Money>
{
    /// <inheritdoc/>
    public override Money Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected a JSON object for Money.");
        }

        decimal? amount = null;
        string? currency = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected a property name inside Money object.");
            }

            var propertyName = reader.GetString();
            reader.Read();

            if (string.Equals(propertyName, "amount", StringComparison.Ordinal))
            {
                amount = reader.TokenType switch
                {
                    JsonTokenType.String => ParseDecimalString(reader.GetString()!),
                    JsonTokenType.Number => reader.GetDecimal(),
                    JsonTokenType.Null => throw new JsonException("Money.amount must not be null."),
                    _ => throw new JsonException("Money.amount must be a string or number."),
                };
            }
            else if (string.Equals(propertyName, "currency", StringComparison.Ordinal))
            {
                currency = reader.TokenType switch
                {
                    JsonTokenType.String => reader.GetString(),
                    JsonTokenType.Null => throw new JsonException("Money.currency must not be null."),
                    _ => throw new JsonException("Money.currency must be a string."),
                };
            }
            else
            {
                reader.Skip();
            }
        }

        if (amount is null)
        {
            throw new JsonException("Money.amount is required.");
        }

        if (currency is null)
        {
            throw new JsonException("Money.currency is required.");
        }

        return new Money(amount.Value, CurrencyCode.Parse(currency));
    }

    private static decimal ParseDecimalString(string text)
    {
        if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value))
        {
            throw new JsonException("Money.amount is not a valid decimal.");
        }

        return value;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Money value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("amount", value.Amount.ToString("0.0000", CultureInfo.InvariantCulture));
        writer.WriteString("currency", value.Currency.Value);
        writer.WriteEndObject();
    }
}

/// <summary>Helpers for registering <see cref="MoneyJsonConverter"/> on a <see cref="JsonSerializerOptions"/>.</summary>
public static class MoneyJsonExtensions
{
    /// <summary>Adds the building-blocks <see cref="MoneyJsonConverter"/> to <paramref name="opts"/>.</summary>
    public static JsonSerializerOptions AddBuildingBlocksMoneyConverter(this JsonSerializerOptions opts)
    {
        opts.Converters.Add(new MoneyJsonConverter());
        return opts;
    }
}
