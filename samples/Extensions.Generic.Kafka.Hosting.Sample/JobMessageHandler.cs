using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Extensions.Generic.Kafka.Hosting.Sample
{
    class JobMessageHandler : IMessageHandler<string, byte[]>
    {
        readonly ILogger logger;

        public JobMessageHandler(ILogger<JobMessageHandler> logger)
        {
            this.logger = logger;
        }

        public async Task Handle(string key, byte[] value)
        {
            logger.LogInformation("Received message from Kafka");
        }
    }
}
