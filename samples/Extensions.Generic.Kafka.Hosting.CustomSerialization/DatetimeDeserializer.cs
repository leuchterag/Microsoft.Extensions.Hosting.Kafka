using System;
using System.Collections.Generic;
using System.Text;
using Confluent.Kafka;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class DatetimeDeserializer : IDeserializer<System.DateTimeOffset>
    {

        public DateTimeOffset Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull)
            {
                return DateTimeOffset.UtcNow;
            }

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
    }
}
