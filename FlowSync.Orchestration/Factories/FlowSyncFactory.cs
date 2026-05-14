using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Entities;
using FlowSync.Orchestration.Entities.Interfaces;

namespace FlowSync.Orchestration.Factories;

public class FlowSyncFactory(
    IConsumer consumer,
    IProducer producer,
    IFilterService filterService,
    IMessagePool messagePool) : IFlowSyncFactory
{
    public async Task<IFlowSyncStep> CreateFlowSyncStepAsync(FlowSyncConfiguration configuration)
    {
        IFlowSyncStep FlowSyncStep = DetermineFlowSyncStepType(configuration);
        await FlowSyncStep.InitializeAsync();
        return FlowSyncStep;
    }

    private IFlowSyncStep DetermineFlowSyncStepType(FlowSyncConfiguration configuration)
    {
        // Determine filter mode based on configuration
        var hasMessageKey = !string.IsNullOrEmpty(configuration.ConsumingOptions?.ExpectedMessageKey);
        var hasCorrelationId = !string.IsNullOrEmpty(configuration.ConsumingOptions?.ExpectedCorrelationId);

        if (hasMessageKey && hasCorrelationId)
        {
            throw new ArgumentException("Cannot specify both ExpectedMessageKey and ExpectedCorrelationId. Choose one filtering mode.");
        }

        if (hasMessageKey)
        {
            return new MessageKeyFlowSyncStep(consumer, producer, filterService, messagePool, configuration);
        }
        
        // Default to correlation ID mode (backward compatibility)
        return new CorrelationIdFlowSyncStep(consumer, producer, filterService, messagePool, configuration);
    }
}