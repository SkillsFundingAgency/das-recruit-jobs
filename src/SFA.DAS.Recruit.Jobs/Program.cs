using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SFA.DAS.Recruit.Jobs.Core.Extensions;

namespace SFA.DAS.Recruit.Jobs
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ILogger? logger = null;
            try
            {
                var builder = new HostBuilder().Configure();
                var host = builder.Build();
                using (host)
                {
                    logger = (host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(nameof(Program));
                    await host.RunAsync();
                }
            }
            catch (Exception e)
            {
                logger?.LogCritical(e, "Unhandled fatal error occured");
                Console.WriteLine("Unhandled fatal error occured");
                Console.WriteLine(e);
                throw;
            }
        }
    }
}


