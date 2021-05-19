using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;

namespace Extensions.Generic.Kafka.Hosting.CustomHandle
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    hostContext.HostingEnvironment.ApplicationName = "Sample Hostbuilder Custom Kafka Message Handler";
                    hostContext.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
                })
                .UseKafka<string, byte[], HandleWithHeaders>(config =>
                {
                    config.BootstrapServers = new[] { "kafka:9092" };
                })
                .ConfigureServices(container =>
                {
                    container.Configure<KafkaListenerSettings>(config =>
                    {
                        config.Topics = new[] { "topic1" };
                        config.ConsumerGroup = "group1";
                        config.AutoOffsetReset = "Latest";
                        config.AutoCommitIntervall = 5000;
                        config.IsAutocommitEnabled = true;
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
