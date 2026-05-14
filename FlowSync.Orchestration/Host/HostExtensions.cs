using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Receivers;
using FlowSync.Orchestration.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FlowSync.Orchestration.Host;
    
public static class HostExtensions
{
    public static IServiceCollection AddFlowSyncService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IFlowSyncFactory, FlowSyncFactory>();
        serviceCollection.AddSingleton<IFilterService, FilterService>();
        serviceCollection.AddSingleton<IMessagePool, MessagePool>();
        return serviceCollection;
    }
}