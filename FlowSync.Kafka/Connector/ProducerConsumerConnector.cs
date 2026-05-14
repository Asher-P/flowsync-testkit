using FlowSync.Core.Messaging.FilterService;
using KafkaFlow;

namespace FlowSync.Kafka.Connector;

public class ProducerConsumerConnector : IConnector
{
    private readonly IFilterService _filterService;

    public ProducerConsumerConnector(IFilterService filterService)
    {
        _filterService = filterService;
    }
    private event Action<string> NewFlowSyncCreated;  
    public Task CreateNewFlowSyncId(string FlowSyncId)
    {
        _filterService.AddFilter(FlowSyncId);
        NewFlowSyncCreated?.Invoke(FlowSyncId);
        return Task.CompletedTask;
    }
}