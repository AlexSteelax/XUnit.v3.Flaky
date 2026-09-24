namespace Steelax.XUnit.v3.Flaky.Enums;

/// <summary>
/// An disposition that is updated on test cases.
/// </summary>
internal enum FlakyDisposition
{
    /// <summary>
    /// Test has not yet started.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Test is running.
    /// </summary>
    Running,

    /// <summary>
    /// The test was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// The test failed.
    /// </summary>
    Fail,

    /// <summary>
    /// The test was successful.
    /// </summary>
    Success
}
