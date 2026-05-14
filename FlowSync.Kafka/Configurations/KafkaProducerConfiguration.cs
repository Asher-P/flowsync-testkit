using KafkaFlow;

namespace FlowSync.Kafka.Configurations;

public class KafkaProducerConfiguration
{
    public string ProducerTopic { get; set; }
    public ISerializer ProducerSerializer { get; set; }
    
}