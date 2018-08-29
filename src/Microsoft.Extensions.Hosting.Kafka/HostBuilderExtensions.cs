using System.Text;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseKafka<TKey, TMessage>(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, container) =>
                {
                    container.Add(new ServiceDescriptor(typeof(IHostedService), typeof(KafkaListenerService<TKey, TMessage>), ServiceLifetime.Singleton));
                    container.Configure<KafkaListenerSettings>(config =>
                    {
                        config.BootstrapServers = new[] { "localhost:9092" };
                    });
                });

            return hostBuilder;
        }

        public static IHostBuilder UseKafka(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, container) =>
                {
                    container.Add(new ServiceDescriptor(typeof(IDeserializer<string>), new StringDeserializer(Encoding.UTF8)));
                    container.Add(new ServiceDescriptor(typeof(IDeserializer<byte[]>), typeof(ByteArrayDeserializer), ServiceLifetime.Singleton));
                });

            hostBuilder.UseKafka<string, byte[]>();

            return hostBuilder;
        }
    }
}
