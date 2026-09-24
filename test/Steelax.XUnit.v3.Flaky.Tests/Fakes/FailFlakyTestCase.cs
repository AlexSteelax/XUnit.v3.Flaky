using Steelax.XUnit.v3.Flaky.Models;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Tests.Fakes;

/// <summary>
/// Fake flaky test case that always reports fail
/// </summary>
internal class FailFlakyTestCase : FlakyTestCase
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public FailFlakyTestCase()
    {
    }

    // ReSharper disable once ConvertToPrimaryConstructor
    public FailFlakyTestCase(IXunitTestMethod testMethod, string testCaseDisplayName, string uniqueID, bool @explicit,
        int retriesBeforeFail, object?[]? testMethodArguments = null)
        : base(testMethod, testCaseDisplayName, uniqueID, @explicit, retriesBeforeFail, testMethodArguments: testMethodArguments)
    {
    }

    public int RunCount { get; private set; }

    protected override ValueTask<RunSummary> RunAttempt(ExplicitOption explicitOption, IMessageBus messageBus,
        object?[] constructorArguments, ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource, ParallelMode parallelMode, ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings)
    {
        RunCount++;
        messageBus.QueueMessage(new DiagnosticMessage("Failed attempt"));
        return new ValueTask<RunSummary>(new RunSummary() { Total = 1, Failed = 1 });
    }
}
