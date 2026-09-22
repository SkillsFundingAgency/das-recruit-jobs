using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Microsoft.Extensions.Hosting;
using SFA.DAS.ProviderRelationships.Messages.Events;
using SFA.DAS.RAA.Vacancy.AI.Api.Core.Events;
using SFA.DAS.Recruit.Api.Core.Events;
using System.Net;

namespace SFA.DAS.Recruit.Jobs.NServiceBus;

public static class ConfigureNServiceBusExtension
{
    private const string ErrorEndpointName = "SFA.DAS.Recruit.Jobs-error";

    public static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.AdvancedConfiguration.AssemblyScanner().ScanFileSystemAssemblies = false;
            endpointConfiguration.AdvancedConfiguration.CustomDiagnosticsWriter((diagnostics, _) =>
            {
                Console.WriteLine(diagnostics);
                return Task.CompletedTask;
            });
            
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
#pragma warning disable CS0618
            var topology = TopicTopology.MigrateFromSingleDefaultTopic();
            topology.EventToMigrate<ReportCreatedEvent>();
            topology.EventToMigrate<VacancyReviewApprovedEvent>();
            topology.EventToMigrate<VacancyReviewCreatedEvent>();
            topology.EventToMigrate<VacancySubmittedEvent>();
            topology.EventToMigrate<VacancyClosedEvent>();
            topology.EventToMigrate<VacancyApprovedEvent>();
            topology.EventToMigrate<VacancyReferredEvent>();
            topology.EventToMigrate<AiVacancyReviewCompletedEvent>();
            topology.EventToMigrate<UpdatedPermissionsEvent>();
            typeof(AzureServiceBusTransport)
                .GetProperty(nameof(AzureServiceBusTransport.Topology))!
                .GetSetMethod(nonPublic: true)!
                .Invoke(endpointConfiguration.Transport, [topology]);
#pragma warning restore CS0618

#if DEBUG
            var transport = endpointConfiguration.AdvancedConfiguration.UseTransport<LearningTransport>();
            transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")),
                @"src\.learningtransport"));

#endif
        });

        return hostBuilder;
    }


    private static bool IsMessage(Type t) => t is IMessage || IsDasMessage(t, "Messages");
    private static bool IsEvent(Type t) => t is IEvent || IsDasMessage(t, "Events");
    private static bool IsCommand(Type t) => t is ICommand || IsDasMessage(t, "Commands");

    private static bool IsDasMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           t.Namespace.EndsWith(namespaceSuffix);
    
}