using Steelax.XUnit.v3.Flaky.Attributes;
using Steelax.XUnit.v3.Flaky.Models;
using Steelax.XUnit.v3.Flaky.Services;
using Xunit;

namespace Steelax.XUnit.v3.Flaky.Tests.Integration;

/// <summary>
/// Tests checking against behaviors/interactions between <see cref="FlakyFactAttribute"/>, <see cref="FlakyFactDiscoverer"/>
/// and <see cref="FlakyTestCase"/>.
/// </summary>
public class FlakyFactTestCaseTests
{
    [Fact]
    public void WhenUsingFactsSync_ShouldBehaveNormally()
    {
        Assert.True(true);
    }

    [Fact]
    public async Task WhenUsingFactsAsync_ShouldBehaveNormally()
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.True(true);
    }

    [FlakyFact]
    public void WhenUsingFlakyFactSync_ShouldBehaveNormallyOnSuccessfulRun()
    {
        Assert.True(true);
    }

    [FlakyFact]
    public async Task WhenUsingFlakyFactAsync_ShouldBehaveNormallyOnSuccessfulRun()
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.True(true);
    }

    [FlakyFact]
    public void WhenUsingFlakyFactSync_ShouldWorkWithExpectedExceptions()
    {
        var action = ExpectedTestException.ThrowException;

        Assert.Throws<ExpectedTestException>(action);
    }

    [FlakyFact]
    public async Task WhenUsingFlakyFactAsync_ShouldWorkWithExpectedExceptions()
    {
        var action = ExpectedTestException.ThrowException;

        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.Throws<ExpectedTestException>(action);
    }

    [FlakyFact(Skip = "skipping")]
    public async Task WhenUsedWithSkip_ShouldSkip()
    {
        await Task.Delay(10_000, TestContext.Current.CancellationToken);
        Assert.Fail("this assert will always fail, but the test should be skipped so it doesn't matter");
    }
}