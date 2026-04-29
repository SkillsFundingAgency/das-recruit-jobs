using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http;

public class GenericGetRequest(string url): IGetRequest
{
    public string Url => url;
}

public class GenericPostRequest(string url, object? data = null): IPostRequest
{
    public string Url => url;
    public object? Data => data;
}

public class GenericPutRequest(string url, object? data = null): IPutRequest
{
    public string Url => url;
    public object? Data => data;
}

public class GenericDeleteRequest(string url): IDeleteRequest
{
    public string Url => url;
}

public class GenericPatchRequest(string url, object? data = null): IPatchRequest
{
    public string Url => url;
    public object? Data => data;
}