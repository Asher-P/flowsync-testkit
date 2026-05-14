using System.Diagnostics;
using FlowSync.Core.Messaging.Consuming;
using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Models;
using FlowSync.Core.Messaging.Publishing;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Core.Extensions;
using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Entities.Enums;
using FlowSync.Orchestration.Entities.Interfaces;

namespace FlowSync.Orchestration.Entities;

public abstract class BaseFlowSyncStep : IFlowSyncStep
{
    protected readonly IConsumer _consumer;
    protected readonly IMessagePool _messagePool;
    protected readonly IProducer _producer;
    protected readonly FlowSyncConfiguration _configuration;
    protected readonly IFilterService _filterService;

    public FlowSyncType FlowSyncType
    {
        get
        {
            if (_configuration.ConsumeFrom.IsNullOrEmpty())
                return FlowSyncType.ProduceAndForget;
            else if (!_configuration.ProduceTo.IsNullOrEmpty())
                return FlowSyncType.ProduceAndWait;
            else return FlowSyncType.ConsumeOnly;
        }
    }

    protected BaseFlowSyncStep(
        IConsumer consumer,
        IProducer producer,
        IFilterService filterService,
        IMessagePool messagePool,
        FlowSyncConfiguration configuration)
    {
        _producer = producer;
        _configuration = configuration;
        _consumer = consumer;
        _messagePool = messagePool;
        _filterService = filterService;
    }

    public abstract Task InitializeAsync();
    
    protected abstract string GetFlowSyncIdentifier(string topic);
    
    protected abstract string GetClearIdentifier();
    
    protected abstract void AddProducingHeaders(ProducingExtraData producingExtraData);

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message)
    {
        return await ExecuteAsync(key, (IEnumerable<T>)new[] { message }, new ProducingExtraData());
    }

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync()
    {
        return await ExecuteConsumeAsync();
    }

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message,
        ProducingExtraData producingExtraData)
    {
        return await ExecuteAsync(key, (IEnumerable<T>)new[] { message }, producingExtraData);
    }

    public async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key,
        IEnumerable<T> messages,
        ProducingExtraData producingExtraData)
    {
        // Add any mode-specific headers
        AddProducingHeaders(producingExtraData);

        foreach (var message in messages)
        {
            await _producer.ProduceAsync(_configuration.ProduceTo, key, message, producingExtraData);
        }

        var tcs = new TaskCompletionSource<IEnumerable<ResponseContainer>>();

        switch (FlowSyncType)
        {
            case FlowSyncType.ProduceAndForget:
                tcs.SetResult(new List<ResponseContainer>());
                break;
            case FlowSyncType.ProduceAndWait:
                GetTaskResultAsync(tcs);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return tcs;
    }

    private async Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteConsumeAsync()
    {
        var tcs = new TaskCompletionSource<IEnumerable<ResponseContainer>>();
        GetTaskResultAsync(tcs);
        return tcs;
    }

    private async Task GetTaskResultAsync(TaskCompletionSource<IEnumerable<ResponseContainer>> tcs)
    {
        var sw = new Stopwatch();
        sw.Start();

        IEnumerable<string> consumeFlowSyncIds = _configuration.ConsumeFrom
            .Select(GetFlowSyncIdentifier);

        while (sw.Elapsed <= _configuration.ConsumingOptions.TimeOut)
        {
            var responseContainers = _messagePool.GetMessages(consumeFlowSyncIds);
            if (_configuration.ConsumingOptions.MsgReceivedCount > 0 && !responseContainers.IsNullOrEmpty() &&
                responseContainers.Sum(x => x.Messages.Count) >= _configuration.ConsumingOptions.MsgReceivedCount)
            {
                tcs.SetResult(responseContainers);

                _messagePool.ClearFlowSyncMessages(GetClearIdentifier());
                return;
            }

            await Task.Delay(250);
        }

        sw.Stop();
        tcs.SetException(new TimeoutException("Did not receive enough messages within the timeout scope"));
    }
} 