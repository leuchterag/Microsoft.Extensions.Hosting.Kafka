using Confluent.Kafka.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class DatetimeDeserializer : IDeserializer<DateTimeOffset>
    {
        public IEnumerable<KeyValuePair<string, object>> Configure(IEnumerable<KeyValuePair<string, object>> config, bool isKey)
        {
            return config;
        }

        public DateTimeOffset Deserialize(string topic, byte[] data)
        {
            try
            {
                var utf8String = Encoding.UTF8.GetString(data);
                DateTimeOffset datetime;
                if (!DateTimeOffset.TryParse(utf8String, out datetime))
                {
                    throw new FormatException($"Failed to deserialize '{utf8String}' to DateTimeOffset");
                }

                return datetime;
            }
            catch (Exception e)
            {
                throw new FormatException($"General error while parsing byte[] to DateTimeOffset", e);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
