using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Extensions.Generic.Kafka.Hosting.CustomSerialization
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    hostContext.HostingEnvironment.ApplicationName = "Sample Hostbuilder Kafka Lowlevel Consumer";
                    hostContext.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
                })
                .UseKafka(config =>
                {
                    // Configuration for the kafka consumer
                    config.BootstrapServers = new[] { "localhost:29092" };
                    config.Topics = new[] { "topic1" };
                    config.ConsumerGroup = "group1";
                    config.DefaultTopicConfig = new Dictionary<string, object>
                    {
                        { "auto.offset.reset", "smallest" }
                    };
                })
                .ConfigureServices(container =>
                {
                    // The message that matches the
                    container.Add(new ServiceDescriptor(typeof(IKafkaMessageHandler<string, byte[]>), typeof(CustomKafkaMessageHandler), ServiceLifetime.Scoped));
                    container.Add(new ServiceDescriptor(typeof(IMessageHandler<string, JObject>), typeof(JobMessageHandler), ServiceLifetime.Scoped));


                })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddConsole();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
