using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;

namespace Extensions.Generic.Kafka.Hosting.CustomHandle
{
    public class HandleWithHeaders : IKafkaMessageHandler<string, byte[]>
    {
        private readonly ILogger<HandleWithHeaders> logger;

        public HandleWithHeaders(ILogger<HandleWithHeaders> logger)
        {
            this.logger = logger;
        }

        public Task Handle(ConsumeResult<string, byte[]> message)
        {
            if (message.Message.Headers.TryGetLastBytes("traceId", out var traceIdBytes))
            {
                logger.LogInformation($"Received message.Message.with trace ID {Encoding.UTF8.GetString(traceIdBytes)} from Kafka {message.Message.Key} : {Encoding.UTF8.GetString(message.Message.Value)}");
                return Task.CompletedTask;
            }

            logger.LogInformation($"Received message.Message.without trace ID from Kafka {message.Message.Key} : {Encoding.UTF8.GetString(message.Message.Value)}");
            return Task.CompletedTask;
        }
    }
}