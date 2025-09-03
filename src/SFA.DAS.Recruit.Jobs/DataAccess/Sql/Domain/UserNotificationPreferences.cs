namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public enum NotificationTypes
{
     ApplicationSubmitted,
     VacancyApprovedOrRejected,
     VacancyClosingSoon,
     VacancySentForReview,
}

public enum NotificationScope
{
    NotSet,
    UserSubmittedVacancies,
    OrganisationVacancies
}

public enum NotificationFrequency
{
    NotSet,
    Never,
    Immediately,
    Daily,
    Weekly
}