using System.Net;
using AutoFixture.NUnit3;
using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;
using SFA.DAS.Recruit.Jobs.Services;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Services;

public class WhenSendingNotifications
{
    [Test, MoqAutoData]
    public async Task Then_The_Call_Succeeds_And_The_Items_Are_Returned(
        NotificationEmail email,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] NotificationService sut)
    {
        // arrange
        var expectedRequest = new PostSendNotificationRequest(email);
        PostSendNotificationRequest? capturedRequest = null;
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<PostSendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IPostRequest, CancellationToken>((x, _) => capturedRequest = x as PostSendNotificationRequest)
            .ReturnsAsync(new ApiResponse(HttpStatusCode.OK));

        // act
        await sut.SendNotificationAsync(email, CancellationToken.None);

        // assert
        capturedRequest.Should().BeEquivalentTo(expectedRequest);
    }
    
    [Test, MoqAutoData]
    public async Task Then_An_Exception_Is_Thrown_When_The_Call_Fails(
        NotificationEmail email,
        [Frozen] Mock<IJobsOuterClient> jobsOuterClient,
        [Greedy] NotificationService sut)
    {
        // arrange
        jobsOuterClient
            .Setup(x => x.PostAsync(It.IsAny<PostSendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponse(HttpStatusCode.BadRequest));

        // act
        var action = async () => await sut.SendNotificationAsync(email, CancellationToken.None);

        // assert
        await action.Should().ThrowAsync<ApiException>();
    }
}