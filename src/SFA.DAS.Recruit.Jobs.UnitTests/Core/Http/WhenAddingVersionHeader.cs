using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http;

public class WhenAddingVersionHeader
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
}