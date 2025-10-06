using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http;

public class WhenAddingHeaders
{
    [Test, MoqAutoData]
    public void Then_The_Version_Header_Is_Added(string headerVersion)
    {
        // arrange
        var message = new HttpRequestMessage();

        // act
        message.AddVersionHeader(headerVersion);

        // assert
        message.Headers.Should().ContainKey("X-Version");
        message.Headers.GetValues("X-Version").Should().Contain(headerVersion);
    }
    
    [Test, MoqAutoData]
    public void Then_The_Apim_Key_Header_Is_Added(string key)
    {
        // arrange
        var message = new HttpRequestMessage();

        // act
        message.AddApimKeyHeader(key);

        // assert
        message.Headers.Should().ContainKey("Ocp-Apim-Subscription-Key");
        message.Headers.GetValues("Ocp-Apim-Subscription-Key").Should().Contain(key);
    }
}