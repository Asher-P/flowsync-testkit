using FlowSync.Orchestration.Configurations;
using FlowSync.Orchestration.Entities.Interfaces;

namespace FlowSync.Orchestration.Factories;

public interface IFlowSyncFactory
{
    Task<IFlowSyncStep> CreateFlowSyncStepAsync(FlowSyncConfiguration configuration);
}