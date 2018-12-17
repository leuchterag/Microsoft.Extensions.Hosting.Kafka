
using System.Linq;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Kafka
{
    static class KafkaConsumerExtensions
    {
        internal static void AttachLogging<TKey, TValue>(this Consumer<TKey, TValue> consumer, ILogger logger)
        {
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
                    if (partitions.Any())
                    {
                        logger.LogInformation($"Consumer was unassigned from: {partitions.Aggregate((x, y) => $"{x}, {y}")}");
                        consumer.Unassign();
                    }
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
        }
    }
}