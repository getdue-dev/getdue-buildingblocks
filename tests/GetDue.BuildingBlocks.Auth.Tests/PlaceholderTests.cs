using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Auth.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_jwt_handlers_land()
    {
        true.Should().BeTrue();
    }
}
