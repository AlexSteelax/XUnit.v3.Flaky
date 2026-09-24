using System.Diagnostics.CodeAnalysis;
using Steelax.XUnit.v3.Flaky.Enums;
using Steelax.XUnit.v3.Flaky.Tests.Integration;
using Xunit;

namespace Steelax.XUnit.v3.Flaky.Tests;

[SuppressMessage("Usage", "xUnit1004:Test methods should not be skipped")]
public class Tests
{
    private const string SkipNonDirect = "Non-Direct";
    
    [FlakyFact(10, Timeout = 100, Skip = SkipNonDirect)]
    public async Task TimeoutNonShared_NonDirect()
    {
        await Task.Delay(70, TestContext.Current.CancellationToken);
        Assert.True(true);
    }
    
    [Fact]
    public async Task TimeoutNonShared()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(TimeoutNonShared_NonDirect));

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(10, testCase.Attempt);
    }

    [FlakyFact(Skip = SkipNonDirect)]
    public void FactSync_NonDirect()
    {
        Assert.True(true);
    }
    
    [Fact]
    public async Task FactSync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(FactSync_NonDirect));

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyFact(Skip = SkipNonDirect)]
    public async Task FactAsync_NonDirect()
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.True(true);
    }
    
    [Fact]
    public async Task FactAsync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(FactAsync_NonDirect));

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyFact(Skip = SkipNonDirect)]
    public void FactExpectedExceptionSync_NonDirect()
    {
        Assert.Throws<ExpectedTestException>(ExpectedTestException.ThrowException);
    }
    
    [Fact]
    public async Task FactExpectedExceptionSync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(FactExpectedExceptionSync_NonDirect));

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyFact(Skip = SkipNonDirect)]
    public async Task FactExpectedExceptionAsync_NonDirect()
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.Throws<ExpectedTestException>(ExpectedTestException.ThrowException);
    }
    
    [Fact]
    public async Task FactExpectedExceptionAsync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(FactExpectedExceptionAsync_NonDirect));

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyFact(Skip = SkipNonDirect)]
    public void FactFail_NonDirect()
    {
        Assert.Fail("This test is expected to fail on the first attempt");
    }
    
    [Fact]
    public async Task FactFail()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(FactFail_NonDirect));

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(1, result.Failed);
        Assert.Equal(1, testCase.Attempt);
        Assert.Equal(FlakyDisposition.Fail, testCase.FlakyDisposition);
    }

    [FlakyTheory(Skip = SkipNonDirect)]
    [InlineData(true)]
    public void TheorySync_NonDirect(bool value)
    {
        Assert.True(value);
    }
    
    [Fact]
    public async Task TheorySync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(TheorySync_NonDirect), [true]);

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyTheory(Skip = SkipNonDirect)]
    [InlineData(true)]
    public async Task TheoryAsync_NonDirect(bool value)
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);
        Assert.True(value);
    }
    
    [Fact]
    public async Task TheoryAsync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(TheoryAsync_NonDirect), [true]);

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyTheory(Skip = SkipNonDirect)]
    [InlineData(true)]
    public void TheoryExpectedExceptionSync_NonDirect(bool value)
    {
        Assert.True(value);
        Assert.Throws<ExpectedTestException>(ExpectedTestException.ThrowException);
    }
    
    [Fact]
    public async Task TheoryExpectedExceptionSync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(TheoryExpectedExceptionSync_NonDirect), [true]);

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }

    [FlakyTheory(Skip = SkipNonDirect)]
    [InlineData(true)]
    public async Task TheoryExpectedExceptionAsync_NonDirect(bool value)
    {
        await Task.Delay(1, TestContext.Current.CancellationToken);

        Assert.True(value);
        Assert.Throws<ExpectedTestException>(ExpectedTestException.ThrowException);
    }
    
    [Fact]
    public async Task TheoryExpectedExceptionAsync()
    {
        await using var testCase = new MoqFlakyTestCase(nameof(TheoryExpectedExceptionAsync_NonDirect), [true]);

        var result = await testCase.Run(TestContext.Current.CancellationToken);
        
        Assert.Equal(0, result.Failed);
        Assert.Equal(3, testCase.Attempt);
    }
}