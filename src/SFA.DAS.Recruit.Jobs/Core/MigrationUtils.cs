using System.Text.Json;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;

namespace SFA.DAS.Recruit.Jobs.Core;

public static class MigrationUtils
{
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, RecruitJobsDataContext.JsonOptions);
    }   
}