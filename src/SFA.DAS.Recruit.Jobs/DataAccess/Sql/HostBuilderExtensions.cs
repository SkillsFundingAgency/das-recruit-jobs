using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureSqlDb(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            var connectionString = context.Configuration.GetConnectionString("SqlServer");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.Configure<RecruitJobsConfiguration>(options =>
                {
                    options.ConnectionString = connectionString;
                });    
            }
            
            services.AddDbContext<RecruitJobsDataContext>(ServiceLifetime.Transient);
            services.AddScoped<RecruitJobsDataContext>();
            services.AddScoped(provider => new Lazy<RecruitJobsDataContext>(provider.GetService<RecruitJobsDataContext>()!));
        });
    }
}