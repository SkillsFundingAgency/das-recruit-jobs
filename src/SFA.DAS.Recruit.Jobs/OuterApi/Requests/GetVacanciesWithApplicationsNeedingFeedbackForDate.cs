using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.OuterApi.Requests;

public class GetVacanciesWithApplicationsNeedingFeedbackForDate(DateTime date): IGetRequest
{
    public string Url => $"vacancies/requiring-applications-feedback/{date:s}";
}