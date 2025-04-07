using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

[ExcludeFromCodeCoverage]
public class ProfanityList : IReferenceDataItem
{
    public string Id { get; set; }
    public DateTime LastUpdatedDate { get; set; }

    public List<string> Profanities { get; set; }
}