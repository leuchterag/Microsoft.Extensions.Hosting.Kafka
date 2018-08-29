# Microsoft.Extensions.Hosting.Kafka

This is an extension library that enables building Kafka consumer applications based on the generic [`GenericHost`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuilder?view=aspnetcore-2.1) which enables the seamless integration with many other hosting extension like logging, dependency injection, etc.

This extension uses The canonical [c# driver implementation from conffluent inc](https://github.com/confluentinc/confluent-kafka-dotnet).

## Usage

This is a minimal sample which enables consuming messages from one or more topics. It assumes the default case of the `key` to be serialized as `string` and the value to be `byte[]`:

```csharp
public static async Task Main(string[] args)
{
    var host = new HostBuilder()
        .UseConsoleLifetime()
        .UseKafka() // Equivilant to .UseKafka<string, byte[]>(), includes registration of key and value serializers
        .ConfigureServices(container =>
        {
            // The message that matches the 
            container.Add(new ServiceDescriptor(typeof(IMessageHandler<string, byte[]>), typeof(JobMessageHandler), ServiceLifetime.Singleton));

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
        .Build();

    await host.RunAsync();
}
```

Should it be necessary to consume messages with different key/value types these can be provided as follows:
```csharp
public static async Task Main(string[] args)
{
    var host = new HostBuilder()
        .UseKafka<DateTimeOffset, string>() // Equivalent to .UseKafka<string, byte[]>()
        .ConfigureServices(container =>
        {
            // The message that matches the 
            container.Add(new ServiceDescriptor(typeof(IMessageHandler<string, byte[]>), typeof(JobMessageHandler), ServiceLifetime.Singleton));

            // Add the necessary serializers into DI!
            container.Add(new ServiceDescriptor(typeof(IDeserializer<string>), new StringDeserializer(Encoding.UTF8)));
            container.Add(new ServiceDescriptor(typeof(IDeserializer<DateTimeOffset>), typeof(DatetimeDeserializer), ServiceLifetime.Singleton));

```

No matter if `.UseKafka()` or `.UseKafka<TKey, TValue>()` is used, the Kafka consumer requires a handler to be registered the implements `IMessageHandler<TKey, TValue>`:

```csharp
class JobMessageHandler : IMessageHandler<string, byte[]>
{
    readonly ILogger logger;

    public JobMessageHandler(ILogger<JobMessageHandler> logger)
    {
        this.logger = logger;
    }

    public async Task Handle(string key, byte[] value)
    {
        logger.LogInformation("Received message from Kafka");
    }
}
```