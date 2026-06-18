using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Outbox.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_outbox_lands()
    {
        true.Should().BeTrue();
    }
}
