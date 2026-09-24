using System.Collections.Generic;
using System.Threading.Tasks;
using Steelax.XUnit.v3.Flaky.Attributes;
using Steelax.XUnit.v3.Flaky.Interfaces;
using Steelax.XUnit.v3.Flaky.Models;
using Xunit.Sdk;
using Xunit.v3;

namespace Steelax.XUnit.v3.Flaky.Services;

/// <summary>
/// Implementation of <see cref="Xunit.v3.TheoryDiscoverer"/> for handling <see cref="FlakyTheoryAttribute"/> decorated
/// test cases.
/// </summary>
internal sealed class FlakyTheoryDiscoverer : TheoryDiscoverer
{
    /// <inheritdoc />
    protected override ValueTask<IReadOnlyCollection<IXunitTestCase>> CreateTestCasesForDataRow(
        ITestFrameworkDiscoveryOptions discoveryOptions,
        IXunitTestMethod testMethod,
        ITheoryAttribute theoryAttribute,
        Xunit.ITheoryDataRow dataRow,
        object?[] testMethodArguments,
        string? index)
    {
        var retriesBeforeFail = ((IFlakyAttribute)theoryAttribute).RetriesBeforeFail;
        var details = TestIntrospectionHelper.GetTestCaseDetailsForTheoryDataRow(
            discoveryOptions, testMethod, theoryAttribute, dataRow, testMethodArguments, index);
        var traits = TestIntrospectionHelper.GetTraits(testMethod, dataRow);

        IXunitTestCase testCase = new FlakyTestCase(
            details.ResolvedTestMethod,
            details.TestCaseDisplayName,
            details.UniqueID,
            details.Explicit,
            retriesBeforeFail,
            dataRow.Label,
            dataRow.DisableParallelization == true,
            details.SkipExceptions,
            details.SkipReason,
            details.SkipType,
            details.SkipUnless,
            details.SkipWhen,
            traits,
            testMethodArguments,
            details.SourceFilePath,
            details.SourceLineNumber,
            details.Timeout);

        return new ValueTask<IReadOnlyCollection<IXunitTestCase>>([testCase]);
    }
}
