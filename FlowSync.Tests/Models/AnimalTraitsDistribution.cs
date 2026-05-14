namespace FlowSync.Tests.Models;

public class AnimalTraitsDistribution
{
    public Animal Animal { get; set; }
    public int ProviderId { get; set; }
    public List<AnimalTrait> TraitsToDistribute { get; set; }
}
