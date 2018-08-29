using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting.Kafka
{
    public interface IMessageHandler<TKey, TValue>
    {
        Task Handle(TKey key, TValue value);
    }
}
