using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Extensions.Generic.Kafka.Hosting.Sample
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    hostContext.HostingEnvironment.ApplicationName = "Sample Hostbuilder Kafka Consumer Sample";
                    hostContext.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
                })
                .UseKafka() // Equivalent to .UseKafka<string, byte[]>()
                .ConfigureServices(container =>
                {
                    // The message that matches the 
                    container.Add(new ServiceDescriptor(typeof(IMessageHandler<string, byte[]>), typeof(JobMessageHandler), ServiceLifetime.Singleton));

                    // Configuration for the kafka consumer
                    container.Configure<KafkaListenerSettings>(config =>
                    {

                        config.BootstrapServers = new[] { "kafka:9092" };
                        config.Topics = new[] { "topic1" };
                        config.ConsumerGroup = "group1";
                        config.DefaultTopicConfig = new Dictionary<string, object>
                        {
                            { "auto.offset.reset", "smallest" }
                        };
                    });
                })
                .ConfigureLogging((ILoggingBuilder loggingBuilder) =>
                {
                    loggingBuilder.AddConsole();
                    loggingBuilder.AddDebug();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
