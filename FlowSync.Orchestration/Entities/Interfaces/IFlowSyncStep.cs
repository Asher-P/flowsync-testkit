using FlowSync.Core.Messaging.Models;
using FlowSync.Core.Messaging.Publishing.Entities;
using FlowSync.Orchestration.Entities.Enums;

namespace FlowSync.Orchestration.Entities.Interfaces;

public interface IFlowSyncStep : IExecutableStep
{
    FlowSyncType FlowSyncType { get; }
    
    Task InitializeAsync();
    
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message);
    
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message, ProducingExtraData producingExtraData);
    
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, IEnumerable<T> messages, ProducingExtraData producingExtraData);
    
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync();
} 