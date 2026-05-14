using System.Collections.Concurrent;
using FlowSync.Tests.Models;

namespace FlowSync.NUnit.Factories;

public class AnimalTraitFactory
{
    public AnimalTrait GetBaseAnimalTrait(int animalId)
    {
        var trait = new AnimalTrait
        {
            Options = new ConcurrentDictionary<long, TraitOption>(),
            CreationDate = DateTime.UtcNow,
            AnimalId = animalId,
            LastUpdate = DateTime.UtcNow,
            ProcessingGuid = Guid.NewGuid(),
            ProviderId = 8,
            Id = new Random().Next(5000000),
            TraitId = 2,
            Trait = new TraitCategory
            {
                Id = 2,
                Name = "Under/Over",
                CategoryGroupId = 7,
                CategoryGroup = new TraitCategoryGroup
                {
                    Id = 7,
                    Name = "Under/Over",
                    OptionGroupId = 1,
                    PropertiesId = 2,
                    AutoCompleteMissingOptions = true,
                    LogicGroup = CategoryLogicGroup.Range,
                    Properties = null
                },
                IsActive = true
            },
            RobotId = 377
        };

        trait.Options.TryAdd(12345647896, new TraitOption
        {
            Name = "Under",
            Line = "0.5",
            BaseLine = "0.5",
            CurrentPrice = 1.5,
            OptionStatus = OptionStatus.Available,
            LastUpdate = DateTime.UtcNow
        });
        trait.Options.TryAdd(12345647897, new TraitOption
        {
            Name = "Over",
            Line = "0.5",
            BaseLine = "0.5",
            CurrentPrice = 1.8,
            OptionStatus = OptionStatus.Available,
            LastUpdate = DateTime.UtcNow
        });

        return trait;
    }
}
