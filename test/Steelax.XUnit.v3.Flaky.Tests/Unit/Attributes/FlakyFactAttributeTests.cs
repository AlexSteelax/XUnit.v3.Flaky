using Steelax.XUnit.v3.Flaky.Attributes;
using Steelax.XUnit.v3.Flaky.Interfaces;
using Xunit;

namespace Steelax.XUnit.v3.Flaky.Tests.Unit.Attributes;

/// <summary>
/// Tests against the <see cref="FlakyFactAttribute"/> class.
/// </summary>
public class FlakyFactAttributeTests
{
    private FlakyFactAttribute? _sut;

    [Fact]
    public void ctor_WhenGivenNoRetryCount_ShouldUseDefault()
    {
        _sut = new FlakyFactAttribute();

        Assert.Equal(IFlakyAttribute.DefaultRetriesBeforeFail, _sut.RetriesBeforeFail);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(42)]
    public void ctor_WhenGivenRetryCount_ShouldSetRetryCount(int retryCount)
    {
        _sut = new FlakyFactAttribute(retryCount);

        Assert.Equal(retryCount, _sut.RetriesBeforeFail);
    }
}
