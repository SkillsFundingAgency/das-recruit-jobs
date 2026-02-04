using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using SFA.DAS.Recruit.Jobs.Domain.Events;

namespace SFA.DAS.Recruit.Jobs.NServiceBus;

public static class ConfigureNServiceBusExtension
{
    private const string EndpointName = "SFA.DAS.FindApprenticeship.Jobs";
    private const string ErrorEndpointName = $"{EndpointName}-error";

    public static void ConfigureNServiceBus(this IHostBuilder hostBuilder, IConfiguration configuration)
    {
        var connectionString = configuration["DasSharedNServiceBusConfiguration:NServiceBusConnectionString"];
        var license = configuration["DasSharedNServiceBusConfiguration:NServiceBusLicense"];

        hostBuilder.UseNServiceBus(
            endpointName: EndpointName,
            connectionString: connectionString,
            (_, endpointConfiguration) =>
            {
                // Send-only endpoint
                endpointConfiguration.AdvancedConfiguration.SendOnly();
                endpointConfiguration.AdvancedConfiguration.SendFailedMessagesTo(ErrorEndpointName);
                endpointConfiguration.AdvancedConfiguration.Conventions()
                    .DefiningCommandsAs(IsCommand)
                    .DefiningMessagesAs(IsMessage)
                    .DefiningEventsAs(IsEvent);
                endpointConfiguration.AdvancedConfiguration.Conventions()
                    .DefiningEventsAs(t =>
                        t == typeof(VacancyClosedEvent) ||
                        (t.Namespace != null && t.Namespace.StartsWith("SFA.DAS.Recruit.Jobs.Domain.Events"))
                    );

                // License
                if (!string.IsNullOrWhiteSpace(license))
                {
                    endpointConfiguration.AdvancedConfiguration.License(
                        WebUtility.HtmlDecode(license));
                }

#if DEBUG
                var transport = endpointConfiguration.AdvancedConfiguration
                    .UseTransport<LearningTransport>();

                transport.StorageDirectory(
                    Path.Combine(
                        Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().IndexOf("src", StringComparison.CurrentCultureIgnoreCase)],
                        @"src\.learningtransport"));
#endif
            });
    }


    private static bool IsMessage(Type t) => IsDasMessage(t, "Messages");
    private static bool IsEvent(Type t) => IsDasMessage(t, "Events");
    private static bool IsCommand(Type t) => IsDasMessage(t, "Commands");

    private static bool IsDasMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           t.Namespace.StartsWith("Esfa.", StringComparison.CurrentCultureIgnoreCase) &&
           t.Namespace.EndsWith(namespaceSuffix);
}