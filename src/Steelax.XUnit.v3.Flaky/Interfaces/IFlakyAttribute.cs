namespace Steelax.XUnit.v3.Flaky.Interfaces;

/// <summary>
/// The required data for marking a test as flaky.
/// </summary>
internal interface IFlakyAttribute
{
    /// <summary>
    /// The default number of retries before failing a test case.
    /// </summary>
    public const int DefaultRetriesBeforeFail = 3;

    /// <summary>
    /// The number of attempts to retry a test case before deeming it a failed test. 
    /// </summary>
    public int RetriesBeforeFail { get; }
}
