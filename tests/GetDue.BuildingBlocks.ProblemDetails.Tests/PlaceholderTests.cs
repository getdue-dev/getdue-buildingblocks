using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.ProblemDetails.Tests;

public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_problem_details_middleware_lands()
    {
        true.Should().BeTrue();
    }
}
