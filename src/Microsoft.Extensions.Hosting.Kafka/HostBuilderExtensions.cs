﻿using System;
using System.Collections.Generic;
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseKafka<TKey, TMessage>(this IHostBuilder hostBuilder, int maxDegreeOfParallelism = 1)
        {
            return hostBuilder.UseKafka<TKey, TMessage>(maxDegreeOfParallelism: maxDegreeOfParallelism, configureDelegate: config =>
            {
                config.BootstrapServers = new[] { "localhost:9092" };
            });
        }

        public static IHostBuilder UseKafka(this IHostBuilder hostBuilder, int maxDegreeOfParallelism = 1)
        {
            return hostBuilder.UseKafka<string, byte[]>(maxDegreeOfParallelism: maxDegreeOfParallelism);
        }

        public static IHostBuilder UseKafka(this IHostBuilder hostBuilder, Action<KafkaListenerSettings> configureDelegate, int maxDegreeOfParallelism = 1)
        {
            hostBuilder.UseKafka<string, byte[]>(configureDelegate, maxDegreeOfParallelism: maxDegreeOfParallelism);

            return hostBuilder;
        }


        public static IHostBuilder UseKafka<TKey, TValue>(this IHostBuilder hostBuilder, Action<KafkaListenerSettings> configureDelegate = null, Action<ConsumerBuilder<TKey, TValue>> builderConfig = null, int maxDegreeOfParallelism = 1)
        {
            return hostBuilder.UseKafka<TKey, TValue, ForwardingKafkaMessageHandler<TKey, TValue>>(configureDelegate, builderConfig, maxDegreeOfParallelism);
        }

        public static IHostBuilder UseKafka<TKey, TValue, THandler>(this IHostBuilder hostBuilder, Action<KafkaListenerSettings> configureDelegate = null, Action<ConsumerBuilder<TKey, TValue>> builderConfig = null, int maxDegreeOfParallelism = 1)
            where THandler: IKafkaMessageHandler<TKey, TValue>
        {
            hostBuilder.ConfigureServices(
                (__, container) =>
                {
                    container.AddOptions<KafkaListenerSettings>();
                    if (configureDelegate != null)
                    {
                        container.Configure(configureDelegate);
                    }
                    container.Add(new ServiceDescriptor(typeof(IKafkaMessageHandler<TKey, TValue>), typeof(THandler), ServiceLifetime.Scoped));

                    container.AddSingleton<IHostedService>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<KafkaListenerService<TKey, TValue>>>();
                        var kafkaConfig = sp.GetRequiredService<IOptions<KafkaListenerSettings>>().Value;

                        var offsetReset = AutoOffsetReset.Latest;
                        if (!Enum.TryParse(kafkaConfig.AutoOffsetReset, false, out offsetReset))
                        {
                            logger.LogError($"Failed to parse value for AutoOffsetReset {kafkaConfig.AutoOffsetReset}, falling back to {nameof(AutoOffsetReset.Latest)}");
                        }

                        var config = new ConsumerConfig(kafkaConfig.ToDictionary(x => x.Key, x => x.Value?.ToString()))
                        {
                            GroupId = kafkaConfig.ConsumerGroup,
                            BootstrapServers = string.Join(",", kafkaConfig.BootstrapServers),
                            AutoOffsetReset = offsetReset,
                            AutoCommitIntervalMs = kafkaConfig.AutoCommitIntervall,
                            EnableAutoCommit = kafkaConfig.IsAutocommitEnabled,
                        };

                        var builder = new ConsumerBuilder<TKey, TValue>(config)
                            .SetErrorHandler((_, e) =>
                            {
                                logger.LogError($"Error: {e.Reason}");
                            })
                            .SetStatisticsHandler((_, json) =>
                            {
                                logger.LogDebug($"Statistics: {json}");
                            })
                            .SetPartitionsAssignedHandler((c, partitions) =>
                            {
                                logger.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
                            })
                            .SetPartitionsRevokedHandler((c, partitions) =>
                            {
                                logger.LogInformation($"Revoking assignment: [{string.Join(", ", partitions)}]");
                            })
                            .SetLogHandler((c, log) =>
                            {
                                logger.LogDebug($"Log: {log}");
                            })
                            .SetOffsetsCommittedHandler((c, commit) =>
                            {
                                var commits = commit.Offsets.Select(x => $"{x.Topic}:{x.Partition}:{x.Offset}").ToArray();
                                logger.LogDebug($"Offset committed: {string.Join(",", commits)}");
                            });

                        // Invoke external build boostrapping
                        builderConfig?.Invoke(builder);

                        var consumer = builder.Build();

                        return new KafkaListenerService<TKey, TValue>(sp, consumer, kafkaConfig.Topics, maxDegreeOfParallelism, logger);
                    });
                });

            return hostBuilder;
        }
    }
}
