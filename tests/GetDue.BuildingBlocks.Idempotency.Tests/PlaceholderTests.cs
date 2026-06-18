using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Idempotency.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_idempotency_middleware_lands()
    {
        true.Should().BeTrue();
    }
}
