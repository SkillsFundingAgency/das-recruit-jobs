using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace SFA.DAS.Recruit.Jobs.Core.Http;

public static class StringExtensions
{
    public static string WithQueryParams(this string url, params (string key, string? value)[] queryParams)
    {
        return QueryHelpers.AddQueryString(url, queryParams.ToDictionary());
    }
    
    public static string WithQueryParams(this string url, params (string key, StringValues values)[] queryParams)
    {
        return QueryHelpers.AddQueryString(url, queryParams.ToDictionary());
    }
}