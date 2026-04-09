namespace SFA.DAS.Recruit.Jobs.Core.Http;

public interface IApiRequest
{
    string Url { get; }
    string Version => "1";
}

public interface IDataApiRequest: IApiRequest
{
    object? Data { get; }
}

public interface IGetRequest : IApiRequest;
public interface IPostRequest : IDataApiRequest;
public interface IPutRequest : IDataApiRequest;
public interface IDeleteRequest : IApiRequest;
public interface IPatchRequest : IDataApiRequest;