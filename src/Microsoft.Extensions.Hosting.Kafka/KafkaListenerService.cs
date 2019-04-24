using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting.Kafka
{
    class KafkaListenerService<TKey, TValue> : BackgroundService
    {
        IConsumer<TKey, TValue> consumer;
        readonly IServiceProvider serviceProvider;
        readonly ILogger logger;
        readonly IEnumerable<string> topics;

        public KafkaListenerService(
            IServiceProvider serviceProvider,
            IConsumer<TKey, TValue> consumer,
            IEnumerable<string> topics,
            ILogger<KafkaListenerService<TKey, TValue>> logger)
        {
            this.consumer = consumer;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.topics = topics;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            consumer.Subscribe(topics);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () => {
                    
                while (!stoppingToken.IsCancellationRequested)
                {

                    try
                    {
                        var msg = consumer.Consume(stoppingToken);
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
                                    logger.LogError(e, "Message handler failed", e);
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            logger.LogDebug("No messages received");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Terminating Kafka listener...");
                        break;
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Failed to receive message.", e);
                        continue;
                    }
                }
            }).ConfigureAwait(false);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
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
