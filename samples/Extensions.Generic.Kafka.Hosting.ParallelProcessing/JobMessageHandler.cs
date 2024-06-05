using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Extensions.Generic.Kafka.Hosting.ParallelProcessing;

class JobMessageHandler : IMessageHandler<string, byte[]>
{
    readonly ILogger logger;
    readonly Random random = new Random();

    public JobMessageHandler(ILogger<JobMessageHandler> logger)
    {
        this.logger = logger;
    }

    public async Task Handle(string key, byte[] value)
    {
        var waitTime = random.Next(2000, 10000);

        if (waitTime > 8000)
        {
            logger.LogInformation($"Simulating exception ({waitTime})");
            throw new Exception($"Simulated exception ({waitTime})");
        }

        logger.LogInformation($"Received message from Kafka. Waiting {waitTime}ms");
        await Task.Delay(waitTime); // Simulate processing time
        logger.LogInformation($"Finished waiting {waitTime}ms");
    }
}
