using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Kafka.Builders;
using FlowSync.Kafka.Consumers;
using FlowSync.Kafka.Producers;
using FlowSync.Orchestration.Host;
using Microsoft.Extensions.DependencyInjection;

namespace FlowSync.Kafka.Extensions;

public static class HostExtensions
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services)
    {
        services.AddSingleton<IConsumer, KafkaConsumer>();
        return services;
    }
    public static IServiceCollection AddKafkaFlowSync(this IServiceCollection serviceCollection,Action<IKafkaConfigurationBuilder> kafkaConfigurationBuilder)
    {
        serviceCollection.AddFlowSyncService()
            .AddSingleton<IConsumer, KafkaConsumer>()
            .AddSingleton<IProducer, KafkaProducer>();
        serviceCollection.AddSingleton<IKafkaConfigurationBuilder, KafkaConfigurationBuilder>();
        var builder = new KafkaConfigurationBuilder();
        kafkaConfigurationBuilder.Invoke(builder);
        builder.BuildKafkaFlow(serviceCollection);
        return serviceCollection;
    }
    
   
}