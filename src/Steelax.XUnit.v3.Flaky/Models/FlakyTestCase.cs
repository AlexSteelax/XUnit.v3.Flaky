using Steelax.XUnit.v3.Flaky.Enums;
using Steelax.XUnit.v3.Flaky.Interfaces;
using Steelax.XUnit.v3.Flaky.Services;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Models;

/// <summary>
/// Additional properties / implementation against the base <see cref="XunitTestCase"/> to accommodate rerunning.
/// </summary>
internal sealed class FlakyTestCase : XunitTestCase, ISelfExecutingXunitTestCase
{
    /// <summary>
    /// The default number of retries before failing a test case.
    /// </summary>
    public const int DefaultRetriesBeforeFail = 3;

    private int _retriesBeforeFail;
    
    /// <summary>
    /// Message template string for running a test attempt
    /// </summary>
    private const string MessageTemplateRunningTestAttemptOf = "Running test '{0}'.  Attempt {1} of {2}";

    /// <summary>
    /// Message template string for a failed test case
    /// </summary>
    private const string MessageTemplateTestReportsFailureAfterAttempts = "The test '{0}' reports failure after {1} attempts.";

    /// <summary>
    /// This constructor should not be used.
    /// </summary>
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    // ReSharper disable once MemberCanBeProtected.Global
    public FlakyTestCase() { }

    /// <summary>
    /// Constructor
    /// </summary>
    public FlakyTestCase(
        IXunitTestMethod testMethod,
        string testCaseDisplayName,
        string uniqueId,
        bool @explicit,
        int retriesBeforeFail,
        string? testLabel = null,
        bool disableParallelization = false,
        Type[]? skipExceptions = null,
        string? skipReason = null,
        Type? skipType = null,
        string? skipUnless = null,
        string? skipWhen = null,
        Dictionary<string, HashSet<string>>? traits = null,
        object?[]? testMethodArguments = null,
        string? sourceFilePath = null,
        int? sourceLineNumber = null,
        int? timeout = null)
        : base(testMethod, testCaseDisplayName, uniqueId, @explicit, testLabel, disableParallelization,
            skipExceptions, skipReason, skipType, skipUnless, skipWhen, traits, testMethodArguments,
            sourceFilePath, sourceLineNumber, timeout)
    {
        _retriesBeforeFail = retriesBeforeFail;
    }

    public FlakyDisposition FlakyDisposition { get; private set; }

    /// <summary>
    /// The number of attempts executed so far. Exposed for tests.
    /// </summary>
    internal int Attempt
    {
        get => Volatile.Read(ref field);
        private set;
    }
    
    /// <inheritdoc />
    public async ValueTask<RunSummary> Run(
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings)
    {
        FlakyDisposition = FlakyDisposition.Running;
        
        using var flakyTestMessageBus = new FlakyMessageBus(messageBus);

        while (!cancellationTokenSource.IsCancellationRequested)
        {
            Attempt++;
            
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokenSource.Token);
            
            Xunit.TestContext.Current.SendDiagnosticMessage(MessageTemplateRunningTestAttemptOf, TestCaseDisplayName, Attempt, _retriesBeforeFail);
            
            var summary = await RunAttempt(explicitOption, flakyTestMessageBus, constructorArguments, aggregator, cts, parallelMode, scheduler, methodFixtureMappings);
            
            // An unsuccessful attempt reports the test as failed immediately: flush the buffer and return.
            if (summary.Failed > 0)
            {
                Xunit.TestContext.Current.SendDiagnosticMessage(MessageTemplateTestReportsFailureAfterAttempts, TestCaseDisplayName, Attempt);

                FlakyDisposition = FlakyDisposition.Fail;
                flakyTestMessageBus.Flush();
                return summary;
            }

            // A skipped test should only be reported once, without further attempts.
            if (summary.Skipped > 0)
            {
                flakyTestMessageBus.Flush();
                return summary;
            }

            // A successful attempt on the final allowed attempt: flush the buffer and report success.
            if (Attempt >= _retriesBeforeFail)
            {
                FlakyDisposition = FlakyDisposition.Success;
                flakyTestMessageBus.Flush();
                return summary;
            }

            // A successful attempt prior to the final attempt: clear the buffer so partial results don't surface,
            // and continue running up to the maximum number of attempts.
            Xunit.TestContext.Current.SendDiagnosticMessage("Test '{0}' succeeded on attempt {1}.  Will retry {2} more times to help assure the test is not flaky", TestCaseDisplayName, Attempt, _retriesBeforeFail - Attempt);

            flakyTestMessageBus.Clear();
        }

        // Task was cancelled.
        FlakyDisposition = FlakyDisposition.Cancelled;
        Xunit.TestContext.Current.SendDiagnosticMessage("The test '{0}' run attempt was cancelled.", TestCaseDisplayName);

        return new RunSummary
        {
            Total = 1,
            Skipped = 1,
        };
    }

    /// <summary>
    /// Runs a single attempt of the test case.
    /// </summary>
    private ValueTask<RunSummary> RunAttempt(
        ExplicitOption explicitOption,
        IMessageBus messageBus,
        object?[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource,
        ParallelMode parallelMode,
        ExecutionScheduler scheduler,
        FixtureMappingManager methodFixtureMappings) =>
        XunitRunnerHelper.RunXunitTestCase(this, messageBus, cancellationTokenSource, parallelMode, scheduler, aggregator, explicitOption, constructorArguments, methodFixtureMappings);

    /// <inheritdoc />
    protected override void Serialize(IXunitSerializationInfo data)
    {
        base.Serialize(data);

        data.AddValue(nameof(_retriesBeforeFail), _retriesBeforeFail);
    }

    /// <inheritdoc />
    protected override void Deserialize(IXunitSerializationInfo data)
    {
        base.Deserialize(data);

        _retriesBeforeFail = data.GetValue<int>(nameof(_retriesBeforeFail));
    }
}
