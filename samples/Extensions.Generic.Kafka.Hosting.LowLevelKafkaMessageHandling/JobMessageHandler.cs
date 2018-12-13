using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class JobMessageHandler : IMessageHandler<string, JObject>
    {
        readonly ILogger logger;

        public JobMessageHandler(ILogger<JobMessageHandler> logger)
        {
            this.logger = logger;
        }

        public Task Handle(string key, JObject value)
        {
            logger.LogInformation($"Received message from Kafka: {key} \n{value.ToString()}");
            return Task.CompletedTask;
        }
    }
}
