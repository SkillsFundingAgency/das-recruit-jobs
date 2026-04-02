using Microsoft.AspNetCore.WebUtilities;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public static class StringExtensions
{
    public static string WithQueryParams(this string baseString, params (string key, string? value)[] items)
    {
        return QueryHelpers.AddQueryString(baseString, items.ToDictionary());
    }
}