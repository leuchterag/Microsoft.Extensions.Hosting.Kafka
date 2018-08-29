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

        public bool IsAutocommitEnabled
        {
            get
            {
                var value = this["enable.auto.commit"];
                if (value != null && value is bool)
                {
                    return (bool)value;
                }

                return false;
            }

            set
            {
                this["enable.auto.commit"] = value;
            }
        }

        public int? AutoCommitIntervall
        {
            get => this["enable.auto.commit"] as int?;
            set => this["enable.auto.commit"] = value;
        }

        public IEnumerable<string> BootstrapServers
        {
            get
            {
                return (this["bootstrap.servers"] as string).Split(',');
            }
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
