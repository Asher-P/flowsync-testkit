using FlowSync.Core.Messaging.Models;
using FlowSync.Core.Messaging.Publishing.Entities;

namespace FlowSync.Orchestration.Entities.Interfaces;

public interface IExecutableStep
{
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, T message, ProducingExtraData extraData);
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync<T>(string key, IEnumerable<T> messages, ProducingExtraData extraData);
    Task<TaskCompletionSource<IEnumerable<ResponseContainer>>> ExecuteAsync();

}