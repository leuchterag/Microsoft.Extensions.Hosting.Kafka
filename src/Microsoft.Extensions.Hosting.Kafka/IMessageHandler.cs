using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting.Kafka
{
    public interface IMessageHandler<in TKey, in TValue>
    {
        Task Handle(TKey key, TValue value);
    }
}
