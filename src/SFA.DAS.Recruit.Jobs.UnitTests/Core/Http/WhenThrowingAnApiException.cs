using System.Net;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http;

public class WhenThrowingAnApiException
{
    [Test]
    public void Then_The_Exception_Message_Is_Formatted_Correctly()
    {
        // arrange/act
        var result = new ApiException("Message text", new ApiResponse(false, HttpStatusCode.BadGateway, "Error Content"));

        // assert
        result.Message.Should().Be("Message text. The remote API returned 'BadGateway' with detail 'Error Content'");
    }
    
    [Test]
    public void Then_A_Null_Api_Response_Should_Be_Handled()
    {
        // arrange/act
        var result = new ApiException("Message text", null);

        // assert
        result.Message.Should().Be("Message text. The remote API returned 'unknown' with detail 'unknown'");
    }
}