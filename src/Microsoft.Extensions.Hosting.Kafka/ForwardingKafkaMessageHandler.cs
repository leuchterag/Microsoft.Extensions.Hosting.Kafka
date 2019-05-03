using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Kafka
{
    class ForwardingKafkaMessageHandler<TKey, TMessage> : IKafkaMessageHandler<TKey, TMessage>
    {
        readonly IMessageHandler<TKey, TMessage> messageHandler;

        public ForwardingKafkaMessageHandler(IMessageHandler<TKey, TMessage> messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public Task Handle(ConsumeResult<TKey, TMessage> message)
        {
            return messageHandler.Handle(message.Key, message.Value);
        }
    }
}
