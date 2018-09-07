using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class CustomKafkaMessageHandler<TKey, TMessage> : IKafkaMessageHandler<TKey, TMessage>
    {
        readonly IMessageHandler<TKey, TMessage> messageHandler;
        readonly ILogger logger;

        public CustomKafkaMessageHandler(IMessageHandler<TKey, TMessage> messageHandler, ILogger<CustomKafkaMessageHandler<TKey, TMessage>> logger)
        {
            this.messageHandler = messageHandler;
            this.logger = logger;
        }

        public Task Handle(Message<TKey, TMessage> message)
        {
            logger.LogInformation($"Handling message from Kafka at offset: {message.Offset}");
            return messageHandler.Handle(message.Key, message.Value);
        }
    }
}
