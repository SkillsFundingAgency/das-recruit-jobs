using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;
using SFA.DAS.Recruit.Jobs.Features.ProhibitedContentMigration;

namespace SFA.DAS.Recruit.Jobs.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
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
        
                // Configure the DAS Encoding service
                var dasEncodingConfig = new EncodingConfig { Encodings = [] };
                context.Configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
                services.AddSingleton(dasEncodingConfig);
                services.AddSingleton<IEncodingService, EncodingService>();
                
                // Configure core project dependencies
                // services.AddTransient<,>();
            })
            .ConfigureMongoDb()
            .ConfigureSqlDb()
            .ConfigureApplicationReviewsMigration()
            .ConfigureProhibitedContentMigration();
    }
}