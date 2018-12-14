using System.Threading.Tasks;
using Confluent.Kafka;

namespace Microsoft.Extensions.Hosting.Kafka
{
    class ForwardingKafkaMessageHandler<TKey, TMessage> : IKafkaMessageHandler<TKey, TMessage>
    {
        readonly IMessageHandler<TKey, TMessage> messageHandler;

        public ForwardingKafkaMessageHandler(IMessageHandler<TKey, TMessage> messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public Task Handle(Message<TKey, TMessage> message)
        {
            return messageHandler.Handle(message.Key, message.Value);
        }
    }
}
