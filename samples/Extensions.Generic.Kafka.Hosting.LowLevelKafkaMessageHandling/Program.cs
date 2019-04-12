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
                    config.AutoOffsetReset = "Latest";
                    config.AutoCommitIntervall = 5000;
                    config.IsAutocommitEnabled = true;
                })
                .ConfigureServices(container =>
                {
                    // The message that matches the
                    container.AddScoped<IKafkaMessageHandler<string, byte[]>, CustomKafkaMessageHandler>();
                    container.AddScoped<IMessageHandler<string, JObject>, JobMessageHandler>();
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
