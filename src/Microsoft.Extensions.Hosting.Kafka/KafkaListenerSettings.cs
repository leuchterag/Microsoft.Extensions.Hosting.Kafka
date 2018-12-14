using System.Collections.Generic;

namespace Microsoft.Extensions.Hosting.Kafka
{
    public class KafkaListenerSettings : Dictionary<string, object>
    {
        public string ConsumerGroup
        {
            get => this["group.id"] as string;
            set => this["group.id"] = value;
        }

        /// <summary>
        /// Enable or disable Autocommit
        /// </summary>
        public bool IsAutocommitEnabled
        {
            get
            {
                var value = this["enable.auto.commit"];
                if (value is bool)
                {
                    return (bool)value;
                }

                return false;
            }

            set => this["enable.auto.commit"] = value;
        }

        /// <summary>
        /// Auto commit interval in ms
        /// </summary>
        public int? AutoCommitIntervall
        {
            get => this["auto.commit.interval.ms"] as int?;
            set => this["auto.commit.interval.ms"] = value;
        }

        /// <summary>
        /// Comma separated list of bootstrap servers.
        /// </summary>
        public IEnumerable<string> BootstrapServers
        {
            get => (this["bootstrap.servers"] as string).Split(',');
            set => this["bootstrap.servers"] = string.Join(",", value);
        }

        public IDictionary<string, object> DefaultTopicConfig
        {
            get
            {
                if (!ContainsKey("default.topic.config"))
                {
                    this["default.topic.config"] = new Dictionary<string, object>();
                }

                return this["default.topic.config"] as IDictionary<string, object>;
            }
            set => this["default.topic.config"] = value;
        }

        public IEnumerable<string> Topics { get; set; }
    }
}
