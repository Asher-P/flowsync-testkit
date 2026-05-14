using System.Collections.Concurrent;
using FlowSync.Core.Messaging.Models;

namespace FlowSync.Core.Messaging.Receivers;

public class MessagePool : IMessagePool
{
    private ConcurrentDictionary<string, List<KeyValuePair<object,object>>> cachedMessages = new();

    public List<ResponseContainer> GetMessages(IEnumerable<string> FlowSyncIds)
    {
        var allMessages = new List<ResponseContainer>();
        foreach (var FlowSyncId in FlowSyncIds)
        {
            cachedMessages.TryGetValue(FlowSyncId, out var messages);
            if (messages != null && messages.Count > 0)
            {
                allMessages.Add(new ResponseContainer()
                {
                    FlowSyncId = FlowSyncId,
                    Messages = new List<MessageContainer>(messages.ToMessageContainerList())
                });
            }
        }

        return allMessages;
    }

    public void ClearFlowSyncMessages(string FlowSyncId)
    {
        if (cachedMessages.TryGetValue(FlowSyncId, out _))
        {
            cachedMessages[FlowSyncId] = new List<KeyValuePair<object,object>>();
        }
    }

    public void AddMessage(string FlowSyncId, KeyValuePair<object,object> keyValuePair)
    {
        var value = cachedMessages.GetOrAdd(FlowSyncId, (s => new List<KeyValuePair<object,object>>()));
        value.Add(keyValuePair);
    }
}