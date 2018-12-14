using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting.Kafka
{
    class KafkaListenerService<TKey, TValue> : BackgroundService
    {
        Consumer<TKey, TValue> consumer;
        Task listener;
        readonly IDeserializer<TValue> valueDeserializer;
        readonly IDeserializer<TKey> keyDeserializer;
        readonly IServiceProvider serviceProvider;
        readonly IOptions<KafkaListenerSettings> listenerSettings;
        readonly ILogger logger;

        public KafkaListenerService(
            IDeserializer<TKey> keyDeserializer,
            IDeserializer<TValue> valueDeserializer,
            IServiceProvider serviceProvider,
            IOptions<KafkaListenerSettings> listenerSettings,
            ILogger<KafkaListenerService<TKey, TValue>> logger)
        {
            this.keyDeserializer = keyDeserializer;
            this.valueDeserializer = valueDeserializer;
            this.serviceProvider = serviceProvider;
            this.listenerSettings = listenerSettings;
            this.logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var settings = listenerSettings.Value;

            consumer = new Consumer<TKey, TValue>(settings, keyDeserializer, valueDeserializer);

            consumer.OnPartitionsAssigned +=
               (_, parts) =>
               {
                   logger.LogInformation($"Consumer was assigned to topics: {string.Join(" ,", parts)}");
                   consumer.Assign(parts);
               };

            consumer.OnPartitionsRevoked +=
                (_, parts) =>
                {
                    var partitions = parts.Select(x => $"{x.Topic}:{x.Partition}");
                    logger.LogInformation($"Consumer was unassigned from: {partitions.Aggregate((x, y) => $"{x}, {y}")}");
                    consumer.Unassign();
                };

            consumer.OnPartitionEOF +=
                (_, end) =>
                {
                    logger.LogInformation($"End of Topic {end.Topic} partition {end.Partition} reached, next offset {end.Offset}");
                };

            consumer.OnError +=
                (_, error) =>
                {
                    logger.LogError($"Listener failed: {error.Code} - {error.Reason}");
                };

            consumer.OnConsumeError +=
                (_, error) =>
                {
                    logger.LogError($"Error encountered while consuming: {error.Error} - {error.Error.Reason}");
                };

            // Subscribe to the given topics
            consumer.Subscribe(settings.Topics.ToList());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                consumer.Consume(out var msg, TimeSpan.FromSeconds(1));
                if (msg != null)
                {
                    logger.LogDebug($"Received message from topic '{msg.Topic}:{msg.Partition}' with offset: '{msg.Offset}[{msg.TopicPartitionOffset}]'");

                    using (var scope = serviceProvider.CreateScope())
                    {
                        var handler = scope.ServiceProvider.GetService<IKafkaMessageHandler<TKey, TValue>>();
                        if (handler == null)
                        {
                            logger.LogError("Failed to resolve message handler. Did you add it to your DI setup.");
                            continue;
                        }
                        try
                        {
                            // Invoke the handler
                            await handler.Handle(msg);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "Message handler failed:");
                            continue;
                        }
                    }
                }
                else
                {
                    logger.LogDebug("No messages received");
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try

                await base.StopAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Kafka listener did not terminated in the allotted time and will be forced.");
                return;
            }

            consumer.Dispose();
            logger.LogInformation("Kafka listener terminated successfully");
        }
    }
}
