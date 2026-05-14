namespace FlowSync.Tests.Models;

public class Animal
{
    public int Id { get; set; }
    public DateTime LastUpdate { get; set; }
    public DateTime StartDate { get; set; }
    public AnimalStatus Status { get; set; }
}
