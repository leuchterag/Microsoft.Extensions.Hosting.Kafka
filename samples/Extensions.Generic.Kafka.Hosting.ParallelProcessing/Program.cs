using Extensions.Generic.Kafka.Hosting.ParallelProcessing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System.IO;

var host = new HostBuilder()
    .UseConsoleLifetime()
    .ConfigureAppConfiguration((hostContext, configApp) =>
    {
        hostContext.HostingEnvironment.ApplicationName = "Sample Hostbuilder Kafka Consumer Sample";
        hostContext.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
    })
    .UseKafka(maxDegreeOfParallelism: 2, configureDelegate: config => // Equivalent to .UseKafka<string, byte[]>()
    {
        config.BootstrapServers = new[] { "broker:9092" };
    })
    .ConfigureServices(container =>
    {
        // The message that matches the 
        container.AddScoped<IMessageHandler<string, byte[]>, JobMessageHandler>();

        // Additional configuration
        container.Configure<KafkaListenerSettings>(config =>
        {
            config.Topics = new[] { "system.messages" };
            config.ConsumerGroup = "group16";
            config.AutoOffsetReset = "Earliest";
            config.AutoCommitIntervall = 2000;
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