using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace SFA.DAS.Recruit.Jobs.NServiceBus;

public class NServiceBusHostedService(IEndpointInstance endpoint) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return endpoint.Stop();
    }
}