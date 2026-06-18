namespace GetDue.BuildingBlocks.Money;

/// <summary>Thrown when an operation combines two <see cref="Money"/> values of different currencies.</summary>
/// <remarks>The message intentionally never contains amounts (SEC-DATA-05).</remarks>
public sealed class CurrencyMismatchException : InvalidOperationException
{
    /// <summary>Currency code of the left-hand operand.</summary>
    public string LeftCurrency { get; }

    /// <summary>Currency code of the right-hand operand.</summary>
    public string RightCurrency { get; }

    /// <summary>Creates a new <see cref="CurrencyMismatchException"/>.</summary>
    public CurrencyMismatchException(string leftCurrency, string rightCurrency)
        : base($"Cannot operate on Money with different currencies: {leftCurrency} vs {rightCurrency}.")
    {
        LeftCurrency = leftCurrency;
        RightCurrency = rightCurrency;
    }
}
