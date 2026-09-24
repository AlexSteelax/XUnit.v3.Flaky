using Steelax.XUnit.v3.Flaky.Attributes;
using Steelax.XUnit.v3.Flaky.Models;
using Steelax.XUnit.v3.Flaky.Services;
using Xunit;

namespace Steelax.XUnit.v3.Flaky.Tests.Integration;

/// <summary>
/// Tests checking against behaviors/interactions between <see cref="FlakyTheoryAttribute"/>, <see cref="FlakyTheoryDiscoverer"/>
/// and <see cref="FlakyTestCase"/>.
/// </summary>
public class FlakyTheoryTestCaseTests
{
    [Theory]
    [InlineData(true)]
    public void WhenUsingTheorySync_ShouldBehaveNormally(bool value)
    {
        Assert.True(value);
    }

    [Theory]
    [InlineData(true)]
    public async Task WhenUsingTheoryAsync_ShouldBehaveNormally(bool value)
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.True(value);
    }

    [FlakyTheory]
    [InlineData(true)]
    public void WhenUsingFlakyTheorySync_ShouldBehaveNormallyOnSuccessfulRun(bool value)
    {
        Assert.True(value);
    }

    [FlakyTheory]
    [InlineData(true)]
    public async Task WhenUsingFlakyTheoryAsync_ShouldBehaveNormallyOnSuccessfulRun(bool value)
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.True(value);
    }

    [FlakyTheory]
    [InlineData(true)]
    public void WhenUsingFlakyTheorySync_ShouldWorkWithExpectedExceptions(bool value)
    {
        var action = ExpectedTestException.ThrowException;

        Assert.True(value);
        Assert.Throws<ExpectedTestException>(action);
    }

    [FlakyTheory]
    [InlineData(true)]
    public async Task WhenUsingFlakyTheoryAsync_ShouldWorkWithExpectedExceptions(bool value)
    {
        var action = ExpectedTestException.ThrowException;

        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.True(value);
        Assert.Throws<ExpectedTestException>(action);
    }

    [FlakyTheory(Skip = "skipping")]
    [InlineData(true, Skip = "skipping")]
    public async Task WhenUsedWithSkipTheory_ShouldSkip(bool value)
    {
        await Task.Delay(10_000, TestContext.Current.CancellationToken);
        Assert.False(value, "this assert will always fail, but the test should be skipped so it doesn't matter");
    }

    [FlakyTheory]
    [InlineData(true, Skip = "skipping")]
    public async Task WhenUsedWithSkipInlineData_ShouldSkip(bool value)
    {
        await Task.Delay(10_000, TestContext.Current.CancellationToken);
        Assert.False(value, "this assert will always fail, but the test should be skipped so it doesn't matter");
    }
}