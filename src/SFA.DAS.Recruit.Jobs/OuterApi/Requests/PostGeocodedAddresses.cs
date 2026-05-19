using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public sealed class PostGeocodedAddresses(Guid vacancyId, List<Address> addresses): IPostRequest
{
    public string Url => $"geocoding/vacancies/{vacancyId}/geocoded";
    public object Data => addresses;
}