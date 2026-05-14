namespace FlowSync.Orchestration.Configurations;

public class FlowSyncConfiguration
{
    public IEnumerable<string> ConsumeFrom { get; set; }
    public string ProduceTo { get; set; }
    // public Predicate Filter;

    public ConsumingOptionsConfiguration ConsumingOptions { get; set; }
}

