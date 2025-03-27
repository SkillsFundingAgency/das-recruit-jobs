using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
public class MongoDbConnectionDetails
{
    public string ConnectionString { get; set; } = string.Empty;
}