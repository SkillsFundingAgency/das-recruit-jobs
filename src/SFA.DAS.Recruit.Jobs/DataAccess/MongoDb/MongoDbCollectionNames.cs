using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
internal static class MongoDbCollectionNames
{
    internal const string ReferenceData = "referenceData";
    internal const string Users = "users";
    internal const string Vacancies = "vacancies";
    internal const string VacancyReviews = "vacancyReviews";
    internal const string EmployerProfiles = "employerProfiles";
    internal const string UserNotificationPreferences = "userNotificationPreferences";
    internal const string BlockedOrganisations = "blockedOrganisations";
}