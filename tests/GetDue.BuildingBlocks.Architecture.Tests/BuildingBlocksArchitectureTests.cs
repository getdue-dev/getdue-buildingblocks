using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace GetDue.BuildingBlocks.Architecture.Tests;

public sealed class BuildingBlocksArchitectureTests
{
    private static readonly string[] ForbiddenDomainTerms =
    [
        "BankAccount", "LoanDebt", "MortgageLoan", "Property", "Portfolio",
        "Holding", "FinancialGoal", "ValuationSnapshot", "Household", "User",
        "Insight", "PaymentSchedule"
    ];

    private static readonly string[] ForbiddenServiceAssemblies =
    [
        "GetDue.Identity", "GetDue.Accounts", "GetDue.Debts",
        "GetDue.RealEstate", "GetDue.Stocks", "GetDue.Goals",
        "GetDue.NetWorth", "GetDue.Insights", "GetDue.Contracts"
    ];

    [Fact]
    public void BuildingBlocks_must_not_contain_domain_terminology()
    {
        var assemblies = LoadBuildingBlocksAssemblies();

        var failures = new List<string>();
        foreach (var term in ForbiddenDomainTerms)
        {
            var result = Types.InAssemblies(assemblies)
                .Should().NotHaveNameMatching($".*{term}.*")
                .GetResult();
            if (!result.IsSuccessful)
            {
                var offenders = result.FailingTypeNames ?? Array.Empty<string>();
                failures.Add($"'{term}': {string.Join(", ", offenders)}");
            }
        }

        failures.Should().BeEmpty(
            "buildingblocks must contain no domain symbols; offenders:\n  " + string.Join("\n  ", failures));
    }

    [Fact]
    public void BuildingBlocks_must_not_reference_GetDue_service_assemblies()
    {
        var assemblies = LoadBuildingBlocksAssemblies();

        var result = Types.InAssemblies(assemblies)
            .Should().NotHaveDependencyOnAny(ForbiddenServiceAssemblies)
            .GetResult();

        var offenders = result.FailingTypeNames ?? Array.Empty<string>();
        result.IsSuccessful.Should().BeTrue(
            $"buildingblocks must not depend on any GetDue service or contracts package; offenders: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Test_1_rule_is_load_bearing_and_fails_on_a_planted_domain_symbol()
    {
        // Plant a forbidden name in the test assembly itself, then run a scoped
        // version of the rule against THIS assembly only and assert it FAILS.
        // This proves the regex/filter logic in Test 1 actually catches a real symbol.
        var assemblies = new[] { typeof(BankAccount).Assembly };
        const string failingTerm = "BankAccount";

        var result = Types.InAssemblies(assemblies)
            .Should().NotHaveNameMatching($".*{failingTerm}.*")
            .GetResult();

        result.IsSuccessful.Should().BeFalse(
            "if this assertion passes, Test 1 is broken - the rule no longer catches a planted domain symbol");
        result.FailingTypeNames.Should().Contain(t => t.Contains("BankAccount", StringComparison.Ordinal));
    }

    private static Assembly[] LoadBuildingBlocksAssemblies() =>
    [
        typeof(GetDue.BuildingBlocks.Money.Money).Assembly,
        Assembly.Load("GetDue.BuildingBlocks.Outbox"),
        Assembly.Load("GetDue.BuildingBlocks.Idempotency"),
        Assembly.Load("GetDue.BuildingBlocks.Telemetry"),
        Assembly.Load("GetDue.BuildingBlocks.Auth"),
        Assembly.Load("GetDue.BuildingBlocks.Resilience"),
        Assembly.Load("GetDue.BuildingBlocks.ProblemDetails"),
    ];

    // Planted forbidden type - lives ONLY in the test assembly. Never ships.
    internal sealed class BankAccount;
}
