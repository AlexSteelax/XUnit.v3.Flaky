using Steelax.XUnit.v3.Flaky.Models;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Tests.Fakes;

/// <summary>
/// Fake flaky test case that returns a fail response after an amount of time.
/// </summary>
internal class DelayedFlakyTestCase : FlakyTestCase
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public DelayedFlakyTestCase()
    {
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public DelayedFlakyTestCase(IXunitTestMethod testMethod, string testCaseDisplayName, string uniqueId, bool @explicit,
        int retriesBeforeFail, object?[]? testMethodArguments = null)
        : base(testMethod, testCaseDisplayName, uniqueId, @explicit, retriesBeforeFail, testMethodArguments: testMethodArguments)
    {
    }

    protected override async ValueTask<RunSummary> RunAttempt(ExplicitOption explicitOption, IMessageBus messageBus,
        object?[] constructorArguments, ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource, ParallelMode parallelMode, ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings)
    {
        await Task.Delay(10_000);
        return new RunSummary() { Total = 1, Failed = 1 };
    }
}
