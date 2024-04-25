using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Kafka
{
    class KafkaListenerService<TKey, TValue> : BackgroundService
    {
        readonly IConsumer<TKey, TValue> consumer;
        readonly IServiceProvider serviceProvider;
        readonly ILogger logger;
        readonly IEnumerable<string> topics;
        readonly List<Task> tasks = new List<Task>();
        readonly int maxDegreeOfParallelism;

        public KafkaListenerService(
            IServiceProvider serviceProvider,
            IConsumer<TKey, TValue> consumer,
            IEnumerable<string> topics,
            int maxDegreeOfParallelism,
            ILogger<KafkaListenerService<TKey, TValue>> logger)
        {
            this.consumer = consumer;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            this.topics = topics;
            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            consumer.Subscribe(topics);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (tasks.Any(t => t.IsFaulted))
                    {
                        // an error occurred in at least one of the tasks
                        foreach (var task in tasks.Where(t => t.IsFaulted).ToArray())
                        {
                            logger.LogError(task.Exception.InnerException, "Message handler failed");
                            tasks.Remove(task);
                        }
                    }

                    if (tasks.Count >= maxDegreeOfParallelism)
                    {
                        // do not process more than the specified number of messages in parallel
                        logger.LogDebug($"Waiting for a message handler to finish to fetch the next message since the max degree of parralelism of {maxDegreeOfParallelism} is reached");
                        await Task.WhenAny(tasks);
                        tasks.RemoveAll(t => t.IsCompleted);
                    }

                    try
                    {
                        var msg = consumer.Consume(stoppingToken);
                        if (msg == null)
                        {
                            logger.LogDebug("No messages received");
                            continue;
                        }

                        if (msg.Message != null)
                        {
                            logger.LogDebug($"Received message from topic '{msg.Topic}:{msg.Partition}' with offset: '{msg.Offset}[{msg.TopicPartitionOffset}]'");

                            using (var scope = serviceProvider.CreateScope())
                            {
                                var handler = scope.ServiceProvider.GetService<IKafkaMessageHandler<TKey, TValue>>();
                                if (handler == null)
                                {
                                    logger.LogError("Failed to resolve message handler. Did you add it to your DI setup?");
                                    continue;
                                }
                                try
                                {
                                    // Invoke the handler
                                    tasks.Add(handler.Handle(msg));
                                }
                                catch (Exception e)
                                {
                                    logger.LogError(e, "Message handler failed", e);
                                    continue;
                                }
                            }
                        }

                        // If needed report the end of the partition
                        if (msg.IsPartitionEOF)
                        {
                            logger.LogInformation($"End of topic {msg.Topic} partition {msg.Partition} reached at offset {msg.Offset}");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogInformation("Terminating Kafka listener...");
                        break;
                    }
                    catch (ConsumeException e)
                    {
                        // Consumer errors should generally be ignored (or logged) unless fatal.
                        logger.LogError(e, $"Consume error: {e.Error.Reason}");

                        if (e.Error.IsFatal)
                        {
                            // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                            break;
                        }
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
                var unfinishedTasks = tasks.Where(t => !t.IsCompleted).ToList();

                if (unfinishedTasks.Any())
                {
                    logger.LogInformation("Waiting for message handlers to finish...");
                    await Task.WhenAll(unfinishedTasks);
                }
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Not all unfinished tasks completed successfully");
                return;
            }
        }

        public override void Dispose()
        {
            try
            {
                consumer.Close(); // Commit offsets and leave the group cleanly.
                consumer.Dispose();

                base.Dispose();
            }
            catch (EntryPointNotFoundException)
            {
                // This exception must not be processed in this case
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("Kafka listener did not terminated in the allotted time and will be forced.");
                return;
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Kafka listener did not terminate gracefully");
                return;
            }

            logger.LogInformation("Kafka listener terminated successfully");
        }
    }
}
