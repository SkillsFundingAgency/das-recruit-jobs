using System.Text.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;

namespace SFA.DAS.Recruit.Jobs.Core;

public static class MigrationUtils
{
    public static T? ParseEnumIfNotNull<T>(string? value) where T: struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enum");
        }
        
        return string.IsNullOrWhiteSpace(value) || value.Equals("undefined", StringComparison.InvariantCultureIgnoreCase)
            ? null
            : Enum.Parse<T>(value, true);
    }
    
    public static string? SerializeOrNull<T>(T? obj)
    {
        return obj is null ? null : JsonSerializer.Serialize(obj, RecruitJobsDataContext.JsonOptions);
    }

    public static bool TryDecodeAccountId(this IEncodingService encodingService, string value, out long result)
    {
        try
        {
            return encodingService.TryDecode(value, EncodingType.AccountId, out result);
        }
        catch
        {
            // currently TryDecode throws if null is passed :/  
            result = 0;
            return false;
        }
    }
    
    public static bool TryDecodePublicAccountLegalEntityId(this IEncodingService encodingService, string value, out long result)
    {
        try
        {
            return encodingService.TryDecode(value, EncodingType.PublicAccountLegalEntityId, out result);
        }
        catch
        {
            // currently TryDecode throws if null is passed :/
            result = 0;
            return false;
        }
    }
}