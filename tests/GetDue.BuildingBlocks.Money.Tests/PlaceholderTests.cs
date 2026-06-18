using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Money.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_money_lands()
    {
        true.Should().BeTrue();
    }
}
