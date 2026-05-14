using FlowSync.Core.Messaging.Models;

namespace FlowSync.Core.Messaging.Receivers;

public interface IMessagePool
{
    List<ResponseContainer> GetMessages(IEnumerable<string> FlowSyncIds);
    void ClearFlowSyncMessages(string FlowSyncId);
    void AddMessage(string FlowSyncId, KeyValuePair<object,object> keyValuePair);
}