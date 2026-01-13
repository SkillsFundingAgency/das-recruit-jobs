using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace SFA.DAS.Recruit.Jobs.NServiceBus;

public static class EndpointConfigurationExtensions
{
    private static readonly string[] RecognisedEventMessageNamespaces =
    [
        "SFA.DAS.EmployerAccounts.Messages.Events",
        "SFA.DAS.ProviderRelationships.Messages.Events"
    ];

    public static EndpointConfiguration UseServiceCollection(this EndpointConfiguration config, IServiceCollection services)
    {
        config.UseContainer<ServicesBuilder>(c => c.ExistingServices(services));
        return config;
    }

    public static EndpointConfiguration UseDasMessageConventions(this EndpointConfiguration config)
    {
        var conventions = config.Conventions();
        conventions.DefiningEventsAs(t =>
            t.Namespace != null && RecognisedEventMessageNamespaces.Any(nsPrefix => t.Namespace.StartsWith(nsPrefix)));
        return config;
    }
}