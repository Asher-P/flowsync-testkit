namespace FlowSync.Kafka.Connector;

public interface IConnector
{
    Task CreateNewFlowSyncId(string FlowSyncId);
}