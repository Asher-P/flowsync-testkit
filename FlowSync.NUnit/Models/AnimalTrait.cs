using System.Collections.Concurrent;

namespace FlowSync.Tests.Models;

public class AnimalTrait
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public int TraitId { get; set; }
    public int ProviderId { get; set; }
    public int RobotId { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastUpdate { get; set; }
    public Guid ProcessingGuid { get; set; }
    public TraitCategory Trait { get; set; }
    public ConcurrentDictionary<long, TraitOption> Options { get; set; }
}
