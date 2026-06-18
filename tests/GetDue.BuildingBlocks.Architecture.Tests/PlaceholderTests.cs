using FluentAssertions;
using Xunit;

namespace GetDue.BuildingBlocks.Architecture.Tests;

// The real architecture rules (no business/domain symbols here, layering, etc.) land
// in subtask 4. For now this is just a scaffold so `dotnet test` finds the project.
public sealed class PlaceholderTests
{
    [Fact]
    public void Placeholder_until_real_arch_rules_land()
    {
        true.Should().BeTrue();
    }
}
