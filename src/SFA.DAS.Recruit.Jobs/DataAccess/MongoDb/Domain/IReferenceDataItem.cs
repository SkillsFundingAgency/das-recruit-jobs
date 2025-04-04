namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public interface IReferenceDataItem
{
    string Id { get; set; }
    DateTime LastUpdatedDate { get; set; }
}