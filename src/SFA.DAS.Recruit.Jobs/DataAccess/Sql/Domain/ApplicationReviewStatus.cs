namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public enum ApplicationReviewStatus
{
    New,
    Successful,
    Unsuccessful,
    Shared,
    InReview,
    Interviewing,
    EmployerInterviewing,
    EmployerUnsuccessful,
    PendingShared,
    PendingToMakeUnsuccessful
}