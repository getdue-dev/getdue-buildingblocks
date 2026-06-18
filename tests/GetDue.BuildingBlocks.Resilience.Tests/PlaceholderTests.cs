using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Resilience.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_resilience_presets_land()
    {
        true.Should().BeTrue();
    }
}
