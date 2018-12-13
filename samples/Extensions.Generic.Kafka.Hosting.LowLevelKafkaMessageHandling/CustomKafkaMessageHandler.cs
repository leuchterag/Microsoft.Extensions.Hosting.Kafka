using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class CustomKafkaMessageHandler : IKafkaMessageHandler<string, byte[]>
    {
        readonly IMessageHandler<string, JObject> messageHandler;
        readonly ILogger logger;

        public CustomKafkaMessageHandler(IMessageHandler<string, JObject> messageHandler, ILogger<CustomKafkaMessageHandler> logger)
        {
            this.messageHandler = messageHandler;
            this.logger = logger;
        }

        public async Task Handle(Message<string, byte[]> message)
        {
            logger.LogInformation($"Handling message from Kafka at offset: {message.Offset}");
            using (var stream = new MemoryStream(message.Value))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var content = await reader.ReadToEndAsync();
                var obj = JObject.Parse(content);

                await messageHandler.Handle(message.Key, obj);
            }
        }
    }
}
