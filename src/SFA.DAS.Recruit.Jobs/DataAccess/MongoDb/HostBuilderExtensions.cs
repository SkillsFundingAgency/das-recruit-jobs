using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureMongoDb(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            var mongoConnectionString = context.Configuration.GetConnectionString("MongoDb");
            if (!string.IsNullOrWhiteSpace(mongoConnectionString))
            {
                services.Configure<MongoDbConnectionDetails>(options =>
                {
                    options.ConnectionString = mongoConnectionString;
                });    
            }
            
            MongoDbConventions.RegisterMongoConventions();
        });
    }
}