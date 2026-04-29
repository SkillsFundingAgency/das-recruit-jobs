using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class GetAccountLegalEntitiesRequest(long accountId) : IGetRequest
{
    public string Url => $"employeraccounts/{accountId}/legalentities";
}