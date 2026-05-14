using FlowSync.Tests.Models;

namespace FlowSync.Tests.Factories;

public static class AnimalFactory
{
    private static readonly Random _random = new();

    public static Task<Animal> CreateActiveAnimal()
    {
        var animal = new Animal
        {
            Id = _random.Next(1000000, 9999999),
            Status = AnimalStatus.Active,
            StartDate = DateTime.UtcNow.AddMinutes(-5),
            LastUpdate = DateTime.UtcNow
        };
        return Task.FromResult(animal);
    }

    public static Task<Animal> CreatePendingAnimal()
    {
        var animal = new Animal
        {
            Id = _random.Next(1000000, 9999999),
            Status = AnimalStatus.Pending,
            StartDate = DateTime.UtcNow.AddMinutes(300),
            LastUpdate = DateTime.UtcNow
        };
        return Task.FromResult(animal);
    }

    public static Task<Animal> CreateNearActiveAnimal()
    {
        var animal = new Animal
        {
            Id = _random.Next(1000000, 9999999),
            Status = AnimalStatus.Pending,
            StartDate = DateTime.UtcNow.AddMinutes(2),
            LastUpdate = DateTime.UtcNow
        };
        return Task.FromResult(animal);
    }
}
