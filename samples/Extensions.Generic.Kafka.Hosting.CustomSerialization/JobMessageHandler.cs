using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class JobMessageHandler : IMessageHandler<DateTimeOffset, string>
    {
        readonly ILogger logger;

        public JobMessageHandler(ILogger<JobMessageHandler> logger)
        {
            this.logger = logger;
        }

        public Task Handle(DateTimeOffset key, string value)
        {
            logger.LogInformation("Received message from Kafka");
            return Task.CompletedTask;
        }
    }
}
