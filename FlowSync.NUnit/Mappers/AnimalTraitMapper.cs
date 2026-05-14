using System.Collections.Concurrent;
using AutoMapper;
using FlowSync.Tests.Models;

namespace FlowSync.Tests.Mappers;

public class AnimalTraitMapper : Profile
{
    public AnimalTraitMapper()
    {
        CreateMap<OutlierTrait, AnimalTrait>()
            .AfterMap((outlierTrait, animalTrait, context) =>
            {
                animalTrait.Options = new ConcurrentDictionary<long, TraitOption>();
                foreach (var option in outlierTrait.Options)
                    animalTrait.Options.TryAdd(option.Id, context.Mapper.Map<TraitOption>(option));
            });
        CreateMap<OutlierOption, TraitOption>()
            .ForMember(dest => dest.OptionStatus, opt => opt.MapFrom(src => OptionStatus.Available));
    }
}
