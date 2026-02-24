using Microsoft.Extensions.Hosting;
using System.Net;

namespace SFA.DAS.Recruit.Jobs.NServiceBus;

public static class ConfigureNServiceBusExtension
{
    private const string ErrorEndpointName = "SFA.DAS.Recruit.Jobs-error";

    public static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            //endpointConfiguration.Transport.SubscriptionRuleNamingConvention = AzureRuleNameShortener.Shorten;
            
            endpointConfiguration.AdvancedConfiguration.EnableInstallers();
            endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo(ErrorEndpointName);
            endpointConfiguration.AdvancedConfiguration.Conventions()
                .DefiningCommandsAs(IsCommand)
                .DefiningMessagesAs(IsMessage)
                .DefiningEventsAs(IsEvent);

            var value = config["NServiceBusLicense"];
            if (!string.IsNullOrEmpty(value))
            {
                var decodedLicence = WebUtility.HtmlDecode(value);
                endpointConfiguration.AdvancedConfiguration.License(decodedLicence);    
            }
            

#if DEBUG
            var transport = endpointConfiguration.AdvancedConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")),
                @"src\.learningtransport"));

#endif
        });

        return hostBuilder;
    }


    private static bool IsMessage(Type t) => IsDasMessage(t, "Messages");
    private static bool IsEvent(Type t) => IsDasMessage(t, "Events");
    private static bool IsCommand(Type t) => IsDasMessage(t, "Commands");

    private static bool IsDasMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           t.Namespace.StartsWith("Esfa.", StringComparison.CurrentCultureIgnoreCase) &&
           t.Namespace.EndsWith(namespaceSuffix);
    
}