using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

[ExcludeFromCodeCoverage]
public class ApplicationWorkExperience
{
    public string Employer { get; set; }
    public string JobTitle { get; set; }
    public string Description { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}