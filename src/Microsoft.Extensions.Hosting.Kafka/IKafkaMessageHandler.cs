using Confluent.Kafka;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting.Kafka
{
    public interface IKafkaMessageHandler<TKey, TMessage>
    {
        Task Handle(Message<TKey, TMessage> message);
    }
}
