using Steelax.XUnit.v3.Flaky.Enums;
using Steelax.XUnit.v3.Flaky.Interfaces;
using Steelax.XUnit.v3.Flaky.Models;
using Steelax.XUnit.v3.Flaky.Tests.Fakes;
using FakeItEasy;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Tests.Unit.Models;

/// <summary>
/// Unit tests against <see cref="FlakyTestCase"/>
/// </summary>
public class FlakyTestCaseTests
{
    private static IMessageBus CreateMessageBus()
    {
        var messageBus = A.Fake<IMessageBus>();
        A.CallTo(() => messageBus.QueueMessage(A<IMessageSinkMessage>._))
            .Returns(true);
        return messageBus;
    }

    private static async Task Run(FlakyTestCase sut, IMessageBus messageBus,
        CancellationTokenSource? tokenSource = null)
    {
        var cancellationTokenSource = tokenSource ?? new CancellationTokenSource();
        await ((ISelfExecutingXunitTestCase)sut).Run(
            ExplicitOption.Off,
            messageBus,
            constructorArguments: [],
            aggregator: new ExceptionAggregator(),
            cancellationTokenSource: cancellationTokenSource,
            parallelMode: ParallelMode.All,
            scheduler: null!,
            methodFixtureMappings: null!);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(42)]
    public void ShouldSerializeWithExpectedCalls(int retriesBeforeFail)
    {
        var sut = GetSystemUnderTest(retriesBeforeFail);
        var info = A.Fake<IXunitSerializationInfo>();

        ((IXunitSerializable)sut).Serialize(info);

        A.CallTo(() => info.AddValue(nameof(FlakyTestCase.RetriesBeforeFail), retriesBeforeFail, typeof(int)))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void ShouldDeserializeWithExpectedCalls()
    {
        var testMethod = A.Fake<IXunitTestMethod>();
        var sut = GetSystemUnderTest(7, testMethod);
        var info = A.Fake<IXunitSerializationInfo>();
        A.CallTo(() => info.GetValue(A<string>._)).Returns((object?)null);
        A.CallTo(() => info.GetValue("dn")).Returns(sut.TestCaseDisplayName);
        A.CallTo(() => info.GetValue("tm")).Returns(testMethod);
        A.CallTo(() => info.GetValue("id")).Returns("unique-id");
        A.CallTo(() => info.GetValue("ex")).Returns(false);
        A.CallTo(() => info.GetValue("to")).Returns(0);
        A.CallTo(() => info.GetValue("tma")).Returns(Array.Empty<object>());
        A.CallTo(() => info.GetValue(nameof(FlakyTestCase.RetriesBeforeFail))).Returns(7);

        ((IXunitSerializable)sut).Deserialize(info);

        Assert.Equal(7, sut.RetriesBeforeFail);
    }

    [Fact]
    public async Task ShouldRunAndReportFailureToDispositionProperty()
    {
        var messageBus = CreateMessageBus();

        var sut = GetFailTestCase(1);

        Assert.Equal(FlakyDisposition.NotStarted, sut.FlakyDisposition);
        await Run(sut, messageBus);

        Assert.Equal(FlakyDisposition.Fail, sut.FlakyDisposition);
    }

    [Fact]
    public async Task ShouldRunAndReportSuccessToDispositionProperty()
    {
        var messageBus = CreateMessageBus();

        var sut = GetSuccessTestCase(1);

        Assert.Equal(FlakyDisposition.NotStarted, sut.FlakyDisposition);
        await Run(sut, messageBus);

        Assert.Equal(FlakyDisposition.Success, sut.FlakyDisposition);
    }

    [Fact]
    public async Task ShouldRunEachAttemptAndOnlyFlushFinalSuccessfulAttemptMessages()
    {
        var messageBus = CreateMessageBus();

        var sut = GetSuccessTestCase(3);

        await Run(sut, messageBus);

        Assert.Equal(FlakyDisposition.Success, sut.FlakyDisposition);
        Assert.Equal(3, sut.RunCount);

        // Only the final (successful) attempt should be flushed to the underlying message bus; prior attempts cleared.
        A.CallTo(() => messageBus.QueueMessage(
                A<IDiagnosticMessage>.That.Matches(m => m.Message.Contains("Successful attempt 3"))))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => messageBus.QueueMessage(
                A<IDiagnosticMessage>.That.Matches(m => m.Message.Contains("Successful attempt 1"))))
            .MustNotHaveHappened();
        A.CallTo(() => messageBus.QueueMessage(
                A<IDiagnosticMessage>.That.Matches(m => m.Message.Contains("Successful attempt 2"))))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task ShouldReportFailureAndFlushOnFirstFailedAttempt()
    {
        var messageBus = CreateMessageBus();

        var sut = GetFailTestCase(3);

        await Run(sut, messageBus);

        Assert.Equal(FlakyDisposition.Fail, sut.FlakyDisposition);

        // A failing test should stop after the first failed attempt, without further attempts.
        Assert.Equal(1, sut.RunCount);

        // The failed attempt's buffer should be flushed to the underlying message bus.
        A.CallTo(() => messageBus.QueueMessage(
                A<IDiagnosticMessage>.That.Matches(m => m.Message.Contains("Failed attempt"))))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ShouldRunAndReportCancelledToDispositionProperty()
    {
        var messageBus = CreateMessageBus();

        var tokenSource = new CancellationTokenSource();
        var sut = GetDelayedTestCase(1);

        Assert.Equal(FlakyDisposition.NotStarted, sut.FlakyDisposition);
        await tokenSource.CancelAsync();
        await Run(sut, messageBus, tokenSource);

        Assert.Equal(FlakyDisposition.Cancelled, sut.FlakyDisposition);
    }

    private static FlakyTestCase GetSystemUnderTest(
        int retriesBeforeFail = IFlakyAttribute.DefaultRetriesBeforeFail,
        IXunitTestMethod? testMethod = null)
    {
        return new FlakyTestCase(
            testMethod ?? A.Fake<IXunitTestMethod>(),
            "MockType.MockMethod",
            "unique-id",
            @explicit: false,
            retriesBeforeFail);
    }

    private FailFlakyTestCase GetFailTestCase(int retriesBeforeFail = IFlakyAttribute.DefaultRetriesBeforeFail)
    {
        return new FailFlakyTestCase(
            A.Fake<IXunitTestMethod>(),
            "MockType.MockMethod",
            "unique-id",
            @explicit: false,
            retriesBeforeFail);
    }

    private SuccessFlakyTestCase GetSuccessTestCase(int retriesBeforeFail = IFlakyAttribute.DefaultRetriesBeforeFail)
    {
        return new SuccessFlakyTestCase(
            A.Fake<IXunitTestMethod>(),
            "MockType.MockMethod",
            "unique-id",
            @explicit: false,
            retriesBeforeFail);
    }

    private DelayedFlakyTestCase GetDelayedTestCase(int retriesBeforeFail = IFlakyAttribute.DefaultRetriesBeforeFail)
    {
        return new DelayedFlakyTestCase(
            A.Fake<IXunitTestMethod>(),
            "MockType.MockMethod",
            "unique-id",
            @explicit: false,
            retriesBeforeFail);
    }
}
