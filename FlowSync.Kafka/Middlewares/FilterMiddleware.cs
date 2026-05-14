using System.Text;
using FlowSync.Core.Extensions;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Receivers;
using KafkaFlow;
using Microsoft.Extensions.Logging;

namespace FlowSync.Kafka.Middlewares;

public class FilterMiddleware : IMessageMiddleware
{
    private static readonly string[] CorrelationIdHeaderOptions = new[] { "correlation_id", "CorrelationId" };
    
    private readonly IFilterService _filterService;
    private readonly ILogger<FilterMiddleware> _logger;

    public FilterMiddleware(
        IFilterService filterService, 
        ILogger<FilterMiddleware> logger)
    {
        _filterService = filterService;
        _logger = logger;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        // Try correlation ID first
        var correlationId = GetCorrelationId(context);
        if (correlationId != null)
        {
            var FlowSyncIdentifier = correlationId.AddPrefix($"{context.ConsumerContext.Topic}_");
            var FlowSyncId = _filterService.Filter(FlowSyncIdentifier);
            
            if (FlowSyncId != null)
            {
                context.Items["FlowSyncId"] = FlowSyncId;
                await next(context).ConfigureAwait(false);
                return;
            }
        }

        // Try message key second
        var messageKey = Encoding.UTF8.GetString(context.Message.Key as byte[]);
        if (messageKey != null)
        {
            var FlowSyncIdentifier = messageKey.AddPrefix($"{context.ConsumerContext.Topic}_key_");
            var FlowSyncId = _filterService.Filter(FlowSyncIdentifier);
            
            if (FlowSyncId != null)
            {
                context.Items["FlowSyncId"] = FlowSyncId;
                await next(context).ConfigureAwait(false);
                return;
            }
        }

        //_logger?.LogWarning($"No matching correlation ID or message key found: topic:{context.ConsumerContext.Topic}, partition:{context.ConsumerContext.Partition}, offset:{context.ConsumerContext.Offset}");
    }

    private string? GetCorrelationId(IMessageContext context)
    {
        foreach (var option in CorrelationIdHeaderOptions)
        {
            var correlationId = context.Headers.GetString(option);
            if (correlationId != null) 
                return correlationId;
        }
        return null;
    }
}