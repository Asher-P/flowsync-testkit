using FlowSync.Core.Messaging.FilterService;
using FlowSync.Core.Messaging.Receivers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlowSync.Tests;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
            // services.AddKafkaConsumer()
            //     .AddKafkaProducer()
            //     .UseKafka();
            services.AddSingleton<IFilterService, FilterService>();
            services.AddSingleton<IMessagePool, MessagePool>();

    }  
    
    public void Configure()
    {
    }
}