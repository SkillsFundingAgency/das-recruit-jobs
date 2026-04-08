using SFA.DAS.Recruit.Jobs.Core.Http;
using SFA.DAS.Recruit.Jobs.Domain;
using SFA.DAS.Recruit.Jobs.OuterApi;
using SFA.DAS.Recruit.Jobs.OuterApi.Common;
using SFA.DAS.Recruit.Jobs.OuterApi.Requests;

namespace SFA.DAS.Recruit.Jobs.Services;

public interface IVacancyService
{
    Task<Vacancy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class VacancyService(IJobsOuterClient jobsOuterClient): IVacancyService
{
    public async Task<Vacancy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await jobsOuterClient.GetAsync<DataResponse<Vacancy?>>(new GetVacancyByIdRequest(id), cancellationToken);
        if (response.NotFound())
        {
            return null;
        }

        response.ThrowIfErrored($"Failed to fetch vacancy '{id}'");
        return response.Payload?.Data;
    }
}