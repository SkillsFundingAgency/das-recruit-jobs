using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public sealed class GetGeoCodeRequest(string postCode) : IGetRequest
{
    public string Url => $"geocoding/postcode/{postCode}/geopoint";
}