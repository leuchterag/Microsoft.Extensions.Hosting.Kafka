using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System.Text;
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

        public Task Handle(string key, byte[] value)
        {
            logger.LogInformation($"Received message from Kafka {key} : {Encoding.UTF8.GetString(value)}");
            return Task.CompletedTask;
        }
    }
}
