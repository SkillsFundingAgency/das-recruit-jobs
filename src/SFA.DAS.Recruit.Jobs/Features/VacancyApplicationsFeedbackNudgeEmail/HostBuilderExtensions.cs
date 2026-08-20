using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail.Handlers;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyApplicationsFeedbackNudgeEmail;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureApplicationsFeedbackNudgeFeature(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            services.AddTransient<IApplicationsFeedbackNudgeEmailHandler, ApplicationsFeedbackNudgeEmailHandler>();
        });
    }
}