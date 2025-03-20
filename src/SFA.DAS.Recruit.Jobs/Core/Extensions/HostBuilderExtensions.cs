using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Encoding;
using SFA.DAS.Recruit.Jobs.Core.Services;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql;
using SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

namespace SFA.DAS.Recruit.Jobs.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder Configure(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        return builder
            .SetupInfrastructure()
            .SetupIocContainer();
    }

    private static IHostBuilder SetupInfrastructure(this IHostBuilder builder)
    {
        return builder
            .ConfigureLogging((_, loggingBuilder) =>
            {
                loggingBuilder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddDebug()
                    .AddConsole();
            })
            .ConfigureHostConfiguration(configHost =>  
            {  
                configHost
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddEnvironmentVariables();
#if DEBUG
                configHost
                    .AddJsonFile("appSettings.json", true)
                    .AddJsonFile("appSettings.Development.json", true);               
#endif
            })
            .ConfigureWebJobs(webJobsBuilder =>
            {
                webJobsBuilder
                    .AddAzureStorageCoreServices()
                    .AddAzureStorageQueues()
                    .AddTimers();
            })
            .ConfigureAppConfiguration((hostBuilderContext, configBuilder)=>
            {
                configBuilder
                    .AddAzureTableStorage(
                        options =>
                        {
                            options.ConfigurationKeys = hostBuilderContext.Configuration["ConfigNames"]!.Split(",");
                            options.EnvironmentName = hostBuilderContext.Configuration["Environment"];
                            options.StorageConnectionString = hostBuilderContext.Configuration["ConfigurationStorageConnectionString"];
                            options.PreFixConfigurationKeys = false;
                        }
                    );
            })
            .ConfigureServices((context, services) =>
            {
                services.AddOptions();
                //services.AddSingleton<IQueueProcessorFactory, CustomQueueProcessorFactory>();
                services.Configure<QueuesOptions>(options =>
                {
                    //maximum number of queue messages that are picked up simultaneously to be executed in parallel (default is 16)
                    options.BatchSize = 1;
                    //Maximum number of retries before a queue message is sent to a poison queue (default is 5)
                    options.MaxDequeueCount = 5;
                    //maximum wait time before polling again when a queue is empty (default is 1 minute).
                    options.MaxPollingInterval = TimeSpan.FromSeconds(10);
                });

                //services.AddDasNServiceBus(context.Configuration);
                var appInsightsConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
                if (!string.IsNullOrEmpty(appInsightsConnectionString))
                {
                    services.AddOpenTelemetry().UseAzureMonitor(options =>
                    {
                        options.ConnectionString = appInsightsConnectionString;
                    });
                }
                
                RegisterDasEncodingService(services, context.Configuration);
            })
            .UseConsoleLifetime()
            .ConfigureMongoDb()
            .ConfigureSqlDb()
            .ConfigureApplicationReviewsMigration();
    }
    
    private static IHostBuilder SetupIocContainer(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<ITimeService, TimeService>();
        });
    }
    
    private static void RegisterDasEncodingService(IServiceCollection services, IConfiguration configuration)
    {
        var dasEncodingConfig = new EncodingConfig { Encodings = [] };
        configuration.GetSection(nameof(dasEncodingConfig.Encodings)).Bind(dasEncodingConfig.Encodings);
        services.AddSingleton(dasEncodingConfig);
        services.AddSingleton<IEncodingService, EncodingService>();
    }
}