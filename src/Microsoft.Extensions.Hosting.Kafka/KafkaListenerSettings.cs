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
                if(!ContainsKey("enable.auto.commit"))
                {
                    return false;
                }
                var value = this["enable.auto.commit"];
                if (value is bool b)
                {
                    return b;
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
            get
            {
                return ContainsKey("auto.commit.interval.ms") ? this["auto.commit.interval.ms"] as int? : new int?();
            }
            set => this["auto.commit.interval.ms"] = value;
        }

        /// <summary>
        /// Auto reset offset [earliest|latest|none]
        /// </summary>
        public string AutoOffsetReset
        {
            get
            {
                return ContainsKey("auto.offset.reset") ? this["auto.offset.reset"] as string : string.Empty;
            }
            set => this["auto.offset.reset"] = value;
        }

        /// <summary>
        /// Comma separated list of bootstrap servers.
        /// </summary>
        public IEnumerable<string> BootstrapServers
        {
            get => (this["bootstrap.servers"] as string).Split(',');
            set => this["bootstrap.servers"] = string.Join(",", value);
        }

        public IEnumerable<string> Topics { get; set; }
    }
}
