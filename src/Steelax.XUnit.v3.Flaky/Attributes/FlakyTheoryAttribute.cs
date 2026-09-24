using System;
using Steelax.XUnit.v3.Flaky.Interfaces;
using Steelax.XUnit.v3.Flaky.Services;
using Xunit;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Attributes;

/// <summary>
/// <para>
/// Use of this attribute indicates a flaky test.
/// </para>
/// <para>
/// This attribute should be used sparingly, but it can be used to mark a <see cref="TheoryAttribute"/> test as "flaky",
/// which will cause the test runner to attempt to run the test until either:
/// <list type="bullet">
/// <item>The test passes</item>
/// <item>The test fails the number of times as specified by the <see cref="RetriesBeforeFail"/></item>
/// </list>
/// If the test fails up to the maximum retries, it reports as failure.
/// </para>
/// </summary>
[XunitTestCaseDiscoverer(typeof(FlakyTheoryDiscoverer))]
[AttributeUsage(AttributeTargets.Method)]
public class FlakyTheoryAttribute : TheoryAttribute, IFlakyAttribute
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="retriesBeforeFail">The number of retries prior to marking a test as failed.</param>
    public FlakyTheoryAttribute(int retriesBeforeFail = IFlakyAttribute.DefaultRetriesBeforeFail)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(retriesBeforeFail);
        RetriesBeforeFail = retriesBeforeFail;
    }

    /// <inheritdoc />
    public int RetriesBeforeFail { get; }
}
