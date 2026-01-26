using System.Net;

namespace SFA.DAS.Recruit.Jobs.NServiceBus;

public class DasSharedNServiceBusConfiguration
{
    public string? ConnectionString { get; set; }

    public string? NServiceBusLicense
    {
        get => _decodedNServiceBusLicense ??= WebUtility.HtmlDecode(_nServiceBusLicense);
        set => _nServiceBusLicense = value;
    }

    private string? _nServiceBusLicense;
    private string? _decodedNServiceBusLicense;
}