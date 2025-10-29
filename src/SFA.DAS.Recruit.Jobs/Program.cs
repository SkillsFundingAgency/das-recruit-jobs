using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Recruit.Jobs.Core.Configuration;

[assembly: NServiceBusTriggerFunction("SFA.DAS.Recruit.Jobs")]
var host = new HostBuilder()
    .ConfigureRecruitJobs()
    .Build();

host.Run();


[ExcludeFromCodeCoverage]
public partial class Program;