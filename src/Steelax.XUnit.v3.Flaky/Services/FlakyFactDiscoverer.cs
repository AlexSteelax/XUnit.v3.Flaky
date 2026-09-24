using Steelax.XUnit.v3.Flaky.Interfaces;
using Steelax.XUnit.v3.Flaky.Models;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Services;

/// <summary>
/// Implementation of <see cref="Xunit.v3.FactDiscoverer"/> for handling <see cref="Xunit.FlakyFactAttribute"/> decorated
/// test cases.
/// </summary>
internal sealed class FlakyFactDiscoverer : FactDiscoverer
{
    /// <inheritdoc />
    protected override IXunitTestCase CreateTestCase(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        IFactAttribute factAttribute)
    {
        var retriesBeforeFail = ((IFlakyAttribute)factAttribute).RetriesBeforeFail;
        var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

        return new FlakyTestCase(
            details.ResolvedTestMethod,
            details.TestCaseDisplayName,
            details.UniqueID,
            details.Explicit,
            retriesBeforeFail,
            skipExceptions: details.SkipExceptions,
            skipReason: details.SkipReason,
            skipType: details.SkipType,
            skipUnless: details.SkipUnless,
            skipWhen: details.SkipWhen,
            sourceFilePath: details.SourceFilePath,
            sourceLineNumber: details.SourceLineNumber,
            timeout: details.Timeout);
    }
}
