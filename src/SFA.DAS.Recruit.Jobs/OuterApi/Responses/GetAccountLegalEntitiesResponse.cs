namespace SFA.DAS.Recruit.Jobs.OuterApi.Responses;

public class GetAccountLegalEntitiesResponse
{
    public List<AccountLegalEntity> AccountLegalEntities { get; set; }
}
    
public class AccountLegalEntity
{
    public bool HasLegalAgreement { get; set; }
    public string AccountLegalEntityPublicHashedId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public long AccountLegalEntityId { get; set; }
    public string DasAccountId { get; set; }
    public long LegalEntityId { get; set; }
}