using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Telemetry.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_telemetry_bootstrap_lands()
    {
        true.Should().BeTrue();
    }
}
