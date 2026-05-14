namespace FlowSync.Tests.Models;

public class TraitOption
{
    public string Name { get; set; }
    public string Line { get; set; }
    public string BaseLine { get; set; }
    public double CurrentPrice { get; set; }
    public OptionStatus OptionStatus { get; set; }
    public DateTime LastUpdate { get; set; }
}
