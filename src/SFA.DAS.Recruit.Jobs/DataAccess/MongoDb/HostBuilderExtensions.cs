﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureMongoDb(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            var connectionString = context.Configuration.GetConnectionString("MongoDb");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.Configure<MongoDbConnectionDetails>(options =>
                {
                    options.ConnectionString = connectionString;
                });    
            }
            
            MongoDbConventions.RegisterMongoConventions();
        });
    }
}