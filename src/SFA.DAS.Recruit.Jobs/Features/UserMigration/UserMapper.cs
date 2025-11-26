using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Diagnostics.CodeAnalysis;
using MongoUser = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.User;
using SqlUser = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.User;

namespace SFA.DAS.Recruit.Jobs.Features.UserMigration;

[ExcludeFromCodeCoverage]
public class UserMapper(IEncodingService encodingService)
{
    public SqlUser MapFrom(MongoUser user)
    {
        return new SqlUser
        {
            Id = user.Id,
            IdamsUserId = user.IdamsUserId,
            Name = user.Name,
            Email = user.Email,
            UserType = Enum.Parse<UserType>(user.UserType.ToString()),
            CreatedDate = user.CreatedDate,
            DfEUserId = user.DfEUserId,
            LastSignedInDate = user.LastSignedInDate,
            Ukprn = user.Ukprn,
            EmployerAccounts = user.EmployerAccountIds == null ? [] :  user.EmployerAccountIds?.Select(x => new UserEmployerAccount { UserId = user.Id, EmployerAccountId = encodingService.Decode(x, EncodingType.AccountId) }).ToList() ?? [],
            ClosedVacanciesBlockedProviderAlertDismissedOn = user.ClosedVacanciesBlockedProviderAlertDismissedOn,
            ClosedVacanciesWithdrawnByQaAlertDismissedOn = user.ClosedVacanciesWithdrawnByQaAlertDismissedOn,
            TransferredVacanciesBlockedProviderAlertDismissedOn = user.TransferredVacanciesBlockedProviderAlertDismissedOn,
            TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn = user.TransferredVacanciesEmployerRevokedPermissionAlertDismissedOn,
        };
    }
}