namespace Steelax.XUnit.v3.Flaky.Interfaces;

/// <summary>
/// The required data for marking a test as flaky.
/// </summary>
internal interface IFlakyAttribute
{
    /// <summary>
    /// The number of attempts to retry a test case before deeming it a failed test. 
    /// </summary>
    public int RetriesBeforeFail { get; }
}
