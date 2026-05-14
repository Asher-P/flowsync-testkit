using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using SaslMechanism = Confluent.Kafka.SaslMechanism;
using SecurityProtocol = Confluent.Kafka.SecurityProtocol;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Kafka.Extensions;
using FlowSync.Kafka.Serializers;
using FlowSync.Kafka.Configurations;
using FlowSync.Tests.Consts;
using FlowSync.Tests.Factories;
using FlowSync.Tests.Models;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Factories;
using KafkaFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration.Placeholder;
using Xunit.Abstractions;

namespace FlowSync.Tests;

public class GeneralTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    public IConfigurationRoot Configuration { get; set; }

    private IServiceProvider _provider;

    public GeneralTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        IServiceCollection services = new ServiceCollection();

        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddPlaceholderResolver()
            .Build();

        services.AddSingleton<IConfiguration>(Configuration);
        services.AddSingleton<AnimalTraitFactory>();
        services.AddLogging(loggingBuilder => { loggingBuilder.AddConsole(); });
        services.AddSingleton<IFilterService, FilterService>();
        services.AddSingleton<IMessagePool, MessagePool>();
        services.AddKafkaFlowSync(builder =>
        {
            var kafkaBrokerConfig = Configuration.GetSection("KafkaBrokerConfiguration").Get<KafkaBrokerConfiguration>();

            builder.ConfigureBroker(brokerBuilder =>
                {
                    brokerBuilder.WithBootstrapServers(kafkaBrokerConfig.BootstrapServers)
                        .WithCredentials(kafkaBrokerConfig.Credentials);
                })
                .AddConsumer(consumerBuilder =>
                {
                    consumerBuilder.AddTopic(OutliersConsts.CONSUMER_TOPIC)
                        .AddConsumerGroup(OutliersConsts.CONSUMER_GROUP)
                        .AddConsumingSerializer(new KafkaUTF8Serializer())
                        .AddConsumingType(typeof(OutlierTrait));
                })
                .AddProducer(producerBuilder =>
                {
                    producerBuilder.AddProducerTopic(OutliersConsts.PRODUCER_OUTLIER)
                        .AddProducerSerializer(new KafkaUTF8Serializer());
                });
        });
        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task FlowSyncTest()
    {
        try
        {
            var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
            var FlowSyncService = await FlowSyncFactory.CreateFlowSyncStepAsync(new FlowSyncConfiguration()
            {
                ProduceTo = OutliersConsts.PRODUCER_OUTLIER,
                ConsumeFrom = new[] { OutliersConsts.CONSUMER_TOPIC },
                ConsumingOptions = new ConsumingOptionsConfiguration()
                {
                    TimeOut = TimeSpan.FromSeconds(30),
                    MsgReceivedCount = 1,
                }
            });

            var animalTraitFactory = _provider.GetRequiredService<AnimalTraitFactory>();

            var animal = await AnimalFactory.CreateActiveAnimal();
            var animalTrait = animalTraitFactory.GetBaseAnimalTrait(animal.Id);
            animal.LastUpdate = DateTime.UtcNow;
            var distribution = new AnimalTraitsDistribution
            {
                Animal = animal,
                ProviderId = animalTrait.ProviderId,
                TraitsToDistribute = new List<AnimalTrait> { animalTrait }
            };

            var waitForMessagesTask = await FlowSyncService.ExecuteAsync(
                $"{animalTrait.AnimalId}_{animalTrait.TraitId}", distribution);
            await Task.Delay(1000);

            var messages = await waitForMessagesTask.Task;

            Console.WriteLine(JsonSerializer.Serialize(messages));
        }
        finally
        {
            Console.WriteLine("Clean Up");
            DeleteConsumerGroupAsync().GetAwaiter().GetResult();
        }
    }

    public async Task DeleteConsumerGroupAsync()
    {
        var kafkaBus = _provider.GetRequiredService<IKafkaBus>();
        foreach (var messageConsumer in kafkaBus.Consumers.All)
            await messageConsumer.StopAsync();

        Thread.Sleep(TimeSpan.FromSeconds(10));

        var kafkaBrokerConfig = Configuration.GetSection("KafkaBrokerConfiguration").Get<KafkaBrokerConfiguration>();

        var adminClientConfig = new AdminClientConfig
        {
            BootstrapServers = string.Join(",", kafkaBrokerConfig.BootstrapServers),
        };

        if (Enum.TryParse<SecurityProtocol>(kafkaBrokerConfig.Credentials.SecurityProtocol, true, out var securityProtocol))
            adminClientConfig.SecurityProtocol = securityProtocol;

        if (kafkaBrokerConfig.Credentials.HasCredentials)
        {
            adminClientConfig.SaslUsername = kafkaBrokerConfig.Credentials.Username;
            adminClientConfig.SaslPassword = kafkaBrokerConfig.Credentials.Password;

            if (Enum.TryParse<SaslMechanism>(kafkaBrokerConfig.Credentials.Mechanism, true, out var saslMechanism))
                adminClientConfig.SaslMechanism = saslMechanism;
        }

        var adminClient = new AdminClientBuilder(adminClientConfig).Build();

        await adminClient.ListConsumerGroupsAsync(new ListConsumerGroupsOptions
        {
            MatchStates = new[] { ConsumerGroupState.Empty }
        });
        await kafkaBus.StopAsync();
        while (true)
        {
            try
            {
                await kafkaBus.StopAsync();
                await adminClient.DeleteGroupsAsync(new List<string> { OutliersConsts.CONSUMER_GROUP });
                break;
            }
            catch (DeleteGroupsException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
