using System.Text;
using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.Receivers;
using KafkaFlow;

namespace FlowSync.Kafka.Middlewares;

public class PushMessageMiddleware : IMessageMiddleware
{
    private readonly IMessagePool _messagePool;

    public PushMessageMiddleware(IMessagePool messagePool)
    {
        _messagePool = messagePool;
    }
    public Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        _messagePool.AddMessage(context.Items["FlowSyncId"].ToString(),new KeyValuePair<object, object>(context.Message.Key,context.Message.Value) );
        return next(context);
    }
}