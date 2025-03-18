using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Recruit.Jobs
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices()
                    .AddAzureStorageQueues();
            })
            .ConfigureLogging((context, b) =>
            {
                b.SetMinimumLevel(LogLevel.Information);
                b.AddConsole();
            });

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}


