﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                    hostContext.HostingEnvironment.ApplicationName = "Sample Hostbuilder Kafka Consumer Sample";
                    hostContext.HostingEnvironment.ContentRootPath = Directory.GetCurrentDirectory();
                })
                .UseKafka<DateTimeOffset, string>(config =>
                {

                    config.BootstrapServers = new[] { "kafka:9092" };
                    config.Topics = new[] { "topic1" };
                    config.ConsumerGroup = "group1";
                    config.AutoOffsetReset = "Latest";
                    config.AutoCommitIntervall = 5000;
                    config.IsAutocommitEnabled = true;
                },
                consumerBuild =>
                {
                    consumerBuild.SetKeyDeserializer(new DatetimeDeserializer());
                })
                .ConfigureServices(container =>
                {
                    // The message that matches the 
                    container.Add(new ServiceDescriptor(typeof(IMessageHandler<string, byte[]>), typeof(JobMessageHandler), ServiceLifetime.Singleton));

                    // Add the necessary serializers into DI!
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
