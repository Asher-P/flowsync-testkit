using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;

namespace FlowSync.Orchestration.Entities;

public class CorrelationIdFlowSyncStep : BaseFlowSyncStep
{
    private readonly string _FlowSyncId;

    public CorrelationIdFlowSyncStep(
        IConsumer consumer,
        IProducer producer,
        IFilterService filterService,
        IMessagePool messagePool,
        FlowSyncConfiguration configuration)
        : base(consumer, producer, filterService, messagePool, configuration)
    {
        _FlowSyncId = Guid.NewGuid().ToString();
    }

    public override async Task InitializeAsync()
    {
        if (FlowSyncType == Enums.FlowSyncType.ProduceAndWait)
        {
            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                var consumeFlowSyncId = _FlowSyncId.AddPrefix($"{consumeFrom}_");
                _filterService.AddFilter(consumeFlowSyncId);
            }

            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                await _consumer.StartConsumeAsync(consumeFrom);
            }
        }
        else if (FlowSyncType == Enums.FlowSyncType.ConsumeOnly)
        {
            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                var expectedCorrelationId = _configuration.ConsumingOptions.ExpectedCorrelationId;
                var consumeFlowSyncId = expectedCorrelationId.AddPrefix($"{consumeFrom}_");
                _filterService.AddFilter(consumeFlowSyncId);
                await _consumer.StartConsumeAsync(consumeFrom);
            }
        }
    }

    protected override string GetFlowSyncIdentifier(string topic)
    {
        if (FlowSyncType == Enums.FlowSyncType.ProduceAndWait)
        {
            return _FlowSyncId.AddPrefix($"{topic}_");
        }
        else if (FlowSyncType == Enums.FlowSyncType.ConsumeOnly)
        {
            var expectedCorrelationId = _configuration.ConsumingOptions.ExpectedCorrelationId;
            return expectedCorrelationId.AddPrefix($"{topic}_");
        }
        
        throw new InvalidOperationException($"Unsupported FlowSync type: {FlowSyncType}");
    }

    protected override string GetClearIdentifier()
    {
        return FlowSyncType == Enums.FlowSyncType.ProduceAndWait 
            ? _FlowSyncId 
            : _configuration.ConsumingOptions.ExpectedCorrelationId;
    }

    protected override void AddProducingHeaders(ProducingExtraData producingExtraData)
    {
        // Add correlation ID header for traditional filtering
        producingExtraData.Headers.Add("FlowSyncId", _FlowSyncId);
        producingExtraData.Headers.Add("correlation_id", _FlowSyncId);
    }
} 