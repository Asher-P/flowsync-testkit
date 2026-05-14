using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Configurations;

namespace FlowSync.Orchestration.Entities;

public class MessageKeyFlowSyncStep : BaseFlowSyncStep
{
    private readonly string _expectedMessageKey;

    public MessageKeyFlowSyncStep(
        IConsumer consumer,
        IProducer producer,
        IFilterService filterService,
        IMessagePool messagePool,
        FlowSyncConfiguration configuration)
        : base(consumer, producer, filterService, messagePool, configuration)
    {
        _expectedMessageKey = configuration.ConsumingOptions?.ExpectedMessageKey 
            ?? throw new ArgumentException("ExpectedMessageKey is required for MessageKeyFlowSyncStep");
    }

    public override async Task InitializeAsync()
    {
        if (FlowSyncType == Enums.FlowSyncType.ProduceAndWait || FlowSyncType == Enums.FlowSyncType.ConsumeOnly)
        {
            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                var consumeFlowSyncId = _expectedMessageKey.AddPrefix($"{consumeFrom}_key_");
                _filterService.AddFilter(consumeFlowSyncId);
            }

            foreach (var consumeFrom in _configuration.ConsumeFrom)
            {
                await _consumer.StartConsumeAsync(consumeFrom);
            }
        }
    }

    protected override string GetFlowSyncIdentifier(string topic)
    {
        return _expectedMessageKey.AddPrefix($"{topic}_key_");
    }

    protected override string GetClearIdentifier()
    {
        return _expectedMessageKey;
    }

    protected override void AddProducingHeaders(ProducingExtraData producingExtraData)
    {
        producingExtraData.Headers.Add("FlowSyncId", _configuration.ConsumingOptions.ExpectedMessageKey);
        producingExtraData.Headers.Add("correlation_id", Guid.NewGuid().ToString());
    }
} 