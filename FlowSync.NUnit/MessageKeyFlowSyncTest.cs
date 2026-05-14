using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Kafka.Extensions;
using FlowSync.Kafka.Serializers;
using FlowSync.Kafka.Configurations;
using FlowSync.NUnit.Consts;
using FlowSync.NUnit.Factories;
using FlowSync.Tests.Models;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Factories;
using FlowSync.Orchestration.Host;
using FlowSync.Orchestration.Entities.Interfaces;
using KafkaFlow;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Extensions.Configuration.Placeholder;

namespace FlowSync.NUnit;

public class MessageKeyFlowSyncTest
{
    public IConfigurationRoot Configuration { get; set; }
    private IServiceProvider _provider;

    [SetUp]
    public void SetUp()
    {
        IServiceCollection services = new ServiceCollection();

        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddPlaceholderResolver()
            .Build();

        services.AddFlowSyncService();
        services.AddSingleton<AnimalTraitFactory>();
        services.AddLogging(loggingBuilder => { loggingBuilder.AddConsole(); });
        services.AddSingleton<IConfiguration>(x => Configuration);

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
                        .AddConsumingType(typeof(OutlierTrait))
                        .SetWorkersCount(55)
                        .SetBufferSize(50);
                })
                .AddProducer(producerBuilder =>
                {
                    producerBuilder.AddProducerTopic(OutliersConsts.PRODUCER_OUTLIER)
                        .AddProducerSerializer(new KafkaProtobufSerializer());
                });
        });
        _provider = services.BuildServiceProvider();
    }

    [Test]
    public async Task MessageKeyFlowSyncTest_ProduceWithKeyAndConsumeByKey()
    {
        try
        {
            var animalTraitFactory = _provider.GetRequiredService<AnimalTraitFactory>();

            var animal = await AnimalFactory.CreateActiveAnimal();
            var animalTrait = animalTraitFactory.GetBaseAnimalTrait(animal.Id);
            animal.LastUpdate = DateTime.UtcNow;

            var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
            var FlowSyncService = await FlowSyncFactory.CreateFlowSyncStepAsync(new FlowSyncConfiguration()
            {
                ProduceTo = OutliersConsts.PRODUCER_OUTLIER,
                ConsumeFrom = new[] { OutliersConsts.CONSUMER_TOPIC },
                ConsumingOptions = new ConsumingOptionsConfiguration()
                {
                    TimeOut = TimeSpan.FromSeconds(30),
                    MsgReceivedCount = 1,
                    ExpectedMessageKey = $"Outliers:InPlay:{animal.Id}:{animalTrait.TraitId}"
                }
            });

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

            Console.WriteLine($"Received {messages.Count()} message containers");
            Console.WriteLine(JsonSerializer.Serialize(messages));
        }
        finally
        {
            Console.WriteLine("Clean Up");
            await DeleteConsumerGroupAsync();
        }
    }

    [Ignore("missing information")]
    [Test]
    public async Task MessageKeyFlowSyncTest_ConsumeOnlyMode()
    {
        try
        {
            var FlowSyncFactory = _provider.GetRequiredService<IFlowSyncFactory>();
            var FlowSyncService = await FlowSyncFactory.CreateFlowSyncStepAsync(new FlowSyncConfiguration()
            {
                ConsumeFrom = new[] { OutliersConsts.CONSUMER_TOPIC },
                ConsumingOptions = new ConsumingOptionsConfiguration()
                {
                    TimeOut = TimeSpan.FromSeconds(30),
                    MsgReceivedCount = 1,
                    ExpectedMessageKey = "specific-consume-key"
                }
            });

            await FlowSyncService.ExecuteAsync<string>("", "");
            await Task.Delay(1000);

            Console.WriteLine("FlowSync set up to consume only messages with key: specific-consume-key");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("No message with expected key received - this is expected for this demo");
        }
        finally
        {
            Console.WriteLine("Clean Up");
            await DeleteConsumerGroupAsync();
        }
    }

    [Test]
    public async Task CorrelationIdFlowSyncTest_TraditionalMode()
    {
        try
        {
            var animalTraitFactory = _provider.GetRequiredService<AnimalTraitFactory>();
            var animal = await AnimalFactory.CreateActiveAnimal();
            var animalTrait = animalTraitFactory.GetBaseAnimalTrait(animal.Id);
            animal.LastUpdate = DateTime.UtcNow;

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

            var distribution = new AnimalTraitsDistribution
            {
                Animal = animal,
                ProviderId = animalTrait.ProviderId,
                TraitsToDistribute = new List<AnimalTrait> { animalTrait }
            };

            await FlowSyncService.ExecuteAsync(
                $"{animalTrait.AnimalId}_{animalTrait.TraitId}", distribution);

            await Task.Delay(1000);
            Console.WriteLine("Traditional correlation ID FlowSync test completed");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Traditional correlation ID FlowSync timeout - this is expected for this demo");
        }
        finally
        {
            Console.WriteLine("Clean Up");
            await DeleteConsumerGroupAsync();
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

        if (Enum.TryParse<Confluent.Kafka.SecurityProtocol>(kafkaBrokerConfig.Credentials.SecurityProtocol, true, out var securityProtocol))
            adminClientConfig.SecurityProtocol = securityProtocol;

        if (kafkaBrokerConfig.Credentials.HasCredentials)
        {
            adminClientConfig.SaslUsername = kafkaBrokerConfig.Credentials.Username;
            adminClientConfig.SaslPassword = kafkaBrokerConfig.Credentials.Password;

            if (Enum.TryParse<Confluent.Kafka.SaslMechanism>(kafkaBrokerConfig.Credentials.Mechanism, true, out var saslMechanism))
                adminClientConfig.SaslMechanism = saslMechanism;
        }

        var adminClient = new AdminClientBuilder(adminClientConfig).Build();

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
                if (e.Message.Contains("The group id does not exist"))
                    return;
                Console.WriteLine(e);
            }
        }
    }
}
