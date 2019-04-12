using System;
using System.Collections.Generic;
using System.Text;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    static class DatetimeDeserializer
    {
        public static DateTimeOffset Deserialize(ReadOnlySpan<byte> data, bool isNull)
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
