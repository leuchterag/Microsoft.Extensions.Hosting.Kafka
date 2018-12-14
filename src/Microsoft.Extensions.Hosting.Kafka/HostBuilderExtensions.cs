using System;
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
            return hostBuilder.UseKafka<TKey, TMessage>(config =>
            {
                config.BootstrapServers = new[] { "localhost:9092" };
            });
        }

        public static IHostBuilder UseKafka<TKey, TMessage>(this IHostBuilder hostBuilder, Action<KafkaListenerSettings> configureDelegate)
        {
            hostBuilder.ConfigureServices(
                (hostCtx, container) =>
                {
                    container.Add(new ServiceDescriptor(typeof(IKafkaMessageHandler<TKey, TMessage>), typeof(ForwardingKafkaMessageHandler<TKey, TMessage>), ServiceLifetime.Scoped));
                    container.Add(new ServiceDescriptor(typeof(IHostedService), typeof(KafkaListenerService<TKey, TMessage>), ServiceLifetime.Singleton));
                    container.Configure(configureDelegate);
                });

            return hostBuilder;
        }

        public static IHostBuilder UseKafka(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseKafka(config =>
            {
                config.BootstrapServers = new[] { "localhost:9092" };
            });
        }


        public static IHostBuilder UseKafka(this IHostBuilder hostBuilder, Action<KafkaListenerSettings> configureDelegate)
        {
            hostBuilder.UseKafka<string, byte[]>(configureDelegate);

            hostBuilder.ConfigureServices(
                (hostCtx, container) =>
                {
                    container.Add(new ServiceDescriptor(typeof(IDeserializer<string>), new StringDeserializer(Encoding.UTF8)));
                    container.Add(new ServiceDescriptor(typeof(IDeserializer<byte[]>), typeof(ByteArrayDeserializer), ServiceLifetime.Singleton));
                });

            return hostBuilder;
        }
    }
}
