namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public enum NotificationTypes
{
     VacancyApprovedOrRejectedByDfE,
     VacancyClosingSoon,
     ApplicationSubmitted,
     VacancySentForReview,
     VacancyRejectedByEmployer,
}

public enum NotificationScope
{
    Default,
    UserSubmittedVacancies,
    OrganisationVacancies
}

public enum NotificationFrequency
{
    Default,
    Immediately,
    Daily,
    Weekly
}