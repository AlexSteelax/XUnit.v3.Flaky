using System.Collections.Concurrent;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Services;

/// <summary>
/// Message bus for flaky tests
/// </summary>
/// <remarks>
/// <para>
/// This message bus only writes messages to the default xunit message bus in cases
/// where the test passed, or the maximum retries have been attempted.
/// </para>
/// <para>
/// Such a decorator? interceptor? is needed, because without it, each failed attempt would be written to the test case
/// output.  Going this route, only the *last* attempt of the test case is written. 
/// </para>
/// </remarks>
internal sealed class FlakyMessageBus : IMessageBus
{
    private readonly IMessageBus _messageBus;
    private readonly ConcurrentQueue<IMessageSinkMessage> _messageQueue = new();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="messageBus">The default XUnit message bus to intercept/decorate.</param>
    public FlakyMessageBus(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    /// <summary>
    /// Enqueues a message
    /// </summary>
    /// <param name="message">The message to enqueue</param>
    public bool QueueMessage(IMessageSinkMessage message)
    {
        _messageQueue.Enqueue(message);
        return true;
    }

    /// <summary>
    /// Write the test case messages to the default message bus.  This is only called on successful test case dispositions,
    /// or in cases where the flaky test has failed.
    /// </summary>
    public void Flush()
    {
        while (_messageQueue.TryDequeue(out var message))
        {
            _messageBus.QueueMessage(message);
        }
    }

    /// <summary>
    /// Discards any messages currently buffered.  This is called after a successful attempt that is not the
    /// final attempt, so that partial results from prior attempts do not surface to the default message bus.
    /// </summary>
    public void Clear()
    {
        while (_messageQueue.TryDequeue(out _))
        {
        }
    }

    /// <summary>
    /// Do nothing, underlying bus is managed by xunit
    /// </summary>
    public void Dispose() { }
}
