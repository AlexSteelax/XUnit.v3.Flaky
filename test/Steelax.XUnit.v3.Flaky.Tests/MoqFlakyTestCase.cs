using System.Collections.Concurrent;
using JetBrains.Annotations;
using Steelax.XUnit.v3.Flaky.Enums;
using Steelax.XUnit.v3.Flaky.Interfaces;
using Steelax.XUnit.v3.Flaky.Models;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Tests;

internal sealed class MoqFlakyTestCase(string methodName, object?[]? testMethodArguments = null) : IAsyncDisposable
{
    private readonly FlakyTestCase _flakyTestCase = CreateTestCase(methodName, testMethodArguments);
    private readonly MoqMessageSink _messageSink = new();
    
    private static string CreateUniqueId() => $"{Guid.NewGuid():N}{Guid.NewGuid():N}";

    private static FlakyTestCase CreateTestCase(string methodName, object?[]? testMethodArguments)
    {
        var testClass = TestContext.Current.TestClass as IXunitTestClass;
        var methodInfo = testClass!.Methods.Single(s => s.Name == methodName);
        
        var testMethod = new XunitTestMethod(testClass, methodInfo, []);
        var testAttribute = methodInfo
            .GetCustomAttributes(false)
            .OfType<IFlakyAttribute>()
            .Single();
        
        return new FlakyTestCase(
            testMethod: testMethod,
            testCaseDisplayName: methodInfo.Name,
            uniqueId: CreateUniqueId(),
            @explicit: false,
            retriesBeforeFail: testAttribute.RetriesBeforeFail,
            timeout: (testAttribute as IFactAttribute)?.Timeout,
            testMethodArguments: testMethodArguments
        );
    }
    
    [PublicAPI]
    public int Attempt => _flakyTestCase.Attempt;
    
    [PublicAPI]
    public FlakyDisposition FlakyDisposition => _flakyTestCase.FlakyDisposition;

    [PublicAPI]
    public IReadOnlyCollection<string> Messages => _messageSink.Queue;

    [PublicAPI]
    public async ValueTask<RunSummary> Run(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var messageBus = new MessageBus(CreateUniqueId(), _messageSink);
        await using var executionScheduler = ExecutionScheduler.CreateUnlimited();
        await using var fixtureMappingManager = new FixtureMappingManager("Class");
        
        var result = await _flakyTestCase.Run(
            ExplicitOption.Off,
            messageBus,
            [],
            new ExceptionAggregator(),
            cts,
            ParallelMode.None,
            executionScheduler,
            fixtureMappingManager);

        return result;
    }

    private sealed class MoqMessageSink : IMessageSink
    {
        public readonly ConcurrentQueue<string> Queue = new();
        
        public bool OnMessage(IMessageSinkMessage message)
        {
            var json = message.ToJson();
            
            if (!string.IsNullOrEmpty(json))
                Queue.Enqueue(json);
            
            return true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _flakyTestCase.DisposeAsync();
    }
}