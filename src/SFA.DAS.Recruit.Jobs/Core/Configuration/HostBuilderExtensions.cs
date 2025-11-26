using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using Polly.Retry;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;
using SFA.DAS.Recruit.Jobs.Features.DelayedNotifications;
using SFA.DAS.Recruit.Jobs.Features.EmployerProfilesMigration;
using SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;
using SFA.DAS.Recruit.Jobs.Features.UpdatePermissionsHandling;
using SFA.DAS.Recruit.Jobs.Features.UserMigration;
using SFA.DAS.Recruit.Jobs.Features.UserNotificationPreferencesMigration;
using SFA.DAS.Recruit.Jobs.Features.VacancyMigration;
using SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;
using SFA.DAS.Recruit.Jobs.OuterApi.Clients;

// ReSharper disable SuspiciousTypeConversion.Global

namespace SFA.DAS.Recruit.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    private const string EndpointName = "SFA.DAS.Recruit.Jobs";
    private const string ErrorEndpointName = $"{EndpointName}-error";

    public static IHostBuilder ConfigureRecruitJobs(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .ConfigureFunctionsWebApplication()
            .ConfigureAppConfiguration(appBuilder =>
            {
                appBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables();

                appBuilder.AddJsonFile("local.settings.json", optional: true);

                var config = appBuilder.Build();
                var environmentName = config["Values:EnvironmentName"] ?? config["EnvironmentName"];
                if (!environmentName!.Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
                {
                    appBuilder.AddAzureTableStorage(options =>
                    {
                        options.ConfigurationNameIncludesVersionNumber = true;
                        options.PreFixConfigurationKeys = false;
#if DEBUG
                        options.ConfigurationKeys = config["Values:ConfigNames"].Split(",");
                        options.StorageConnectionString = config["Values:ConfigurationStorageConnectionString"];
                        options.EnvironmentName = config["Values:EnvironmentName"];
#else
                        options.ConfigurationKeys = config["ConfigNames"].Split(",");
                        options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
                        options.EnvironmentName = config["EnvironmentName"];
#endif
                    });
                }
            })
            .ConfigureServices((context, services) =>
            {
                // Setup application insights
                services.AddApplicationInsightsTelemetryWorkerService(options =>
                {
#if DEBUG
                    options.DeveloperMode = true;
#endif
                });
                services.ConfigureFunctionsApplicationInsights();
                services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
                        loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);

                        loggingBuilder.AddFilter(typeof(Program).Namespace, LogLevel.Information);
                        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                        loggingBuilder.AddConsole();
                    }
                );

                services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), context.Configuration));
                services.AddOptions();
                services.ConfigureDependencies(context);
            })
            .ConfigureNServiceBus()
            .ConfigureMongoDb()
            .ConfigureSqlDb()
            .ConfigureApplicationReviewsMigration()
            .ConfigureProhibitedContentMigration()
            .ConfigureUserNotificationPreferencesMigration()
            .ConfigureEmployerProfilesMigration()
            .ConfigureVacancyReviewMigration()
            .ConfigureUserMigration()
            .ConfigureVacancyMigration()
            .ConfigureDelayedNotificationsFeature()
            .ConfigureUpdatePermissionsHandlingFeature();
    }
    
    private static AsyncRetryPolicy<HttpResponseMessage> HttpClientRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3);
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.BadRequest)
            .WaitAndRetryAsync(delay);
    }

    private static void ConfigureDependencies(this IServiceCollection services, HostBuilderContext context)
    {
        // Configure the DAS Encoding service
        var dasEncodingConfig = new EncodingConfig { Encodings = [] };
        context.Configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
        services.AddSingleton(dasEncodingConfig);
        services.AddSingleton<IEncodingService, EncodingService>();
        
        // Setup config classes
        services.Configure<RecruitJobsOuterApiConfiguration>(context.Configuration.GetSection("RecruitJobsOuterApiConfiguration"));
        services.Configure<RecruitJobsConfiguration>(context.Configuration);
        services.AddSingleton(cfg => cfg.GetService<IOptions<RecruitJobsOuterApiConfiguration>>()!.Value);
        services.AddSingleton(cfg => cfg.GetService<IOptions<RecruitJobsConfiguration>>()!.Value);

        // Configure core project dependencies
        var jsonSerializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        services.AddSingleton(jsonSerializationOptions);
        
        services.AddTransient<IUpdatedPermissionsClient, UpdatedPermissionsClient>();
        services
            .AddHttpClient<IUpdatedPermissionsClient, UpdatedPermissionsClient>()
            .SetHandlerLifetime(TimeSpan.FromMinutes(5))
            .AddPolicyHandler(HttpClientRetryPolicy());
    }

    private static IHostBuilder ConfigureNServiceBus(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseNServiceBus((config, endpointConfiguration) =>
        {
            endpointConfiguration.Transport.SubscriptionRuleNamingConvention = AzureRuleNameShortener.Shorten;
            
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
            transport.StorageDirectory(Path.Combine(Directory.GetCurrentDirectory()[..Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)], @"src\.learningtransport"));
#endif
        });

        return hostBuilder;
    }

    private static bool IsMessage(Type t) => t is IMessage || IsDasMessage(t, "Messages");

    private static bool IsEvent(Type t) => t is IEvent || IsDasMessage(t, "Events");

    private static bool IsCommand(Type t) => t is ICommand || IsDasMessage(t, "Commands");

    private static bool IsDasMessage(Type t, string namespaceSuffix)
        => t.Namespace != null &&
           (t.Namespace.StartsWith("Esfa.", StringComparison.CurrentCultureIgnoreCase)) &&
           t.Namespace.EndsWith(namespaceSuffix);
}