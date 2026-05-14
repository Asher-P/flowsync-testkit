using System.Runtime.CompilerServices;

namespace FlowSync.Tests.Models;

public record OutlierTrait
{
    private sealed class OutlierTraitEqualityComparer : IEqualityComparer<OutlierTrait>
    {
        public bool Equals(OutlierTrait x, OutlierTrait y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Name == y.Name && x.AnimalId == y.AnimalId && x.TraitId == y.TraitId && x.ProviderId == y.ProviderId
                && x.Options.OrderBy(b => b.Id).SequenceEqual(y.Options.OrderBy(b2 => b2.Id), OutlierOption.OutlierOptionComparer)
                && x.ProviderAnimalId == y.ProviderAnimalId && x.Key == y.Key && x.RobotId == y.RobotId;
        }

        public int GetHashCode(OutlierTrait obj)
        {
            return HashCode.Combine(obj.Name, obj.AnimalId, obj.TraitId, obj.ProviderId,
                GetListHash(obj.Options, OutlierOption.OutlierOptionComparer), obj.ProviderAnimalId, obj.Key, obj.RobotId);
        }

        private static int GetListHash<T>(IEnumerable<T> list, IEqualityComparer<T> comparer = null)
        {
            var hashCode = new HashCode();
            foreach (var item in list ?? Enumerable.Empty<T>())
                hashCode.Add(item, comparer);
            return hashCode.ToHashCode();
        }
    }

    public static IEqualityComparer<OutlierTrait> OutlierTraitComparer { get; } = new OutlierTraitEqualityComparer();

    public OutlierTrait(OutlierTrait source)
    {
        Id = source.Id;
        Name = source.Name;
        AnimalId = source.AnimalId;
        TraitId = source.TraitId;
        ProviderId = source.ProviderId;
        Options = [];
        LastUpdate = source.LastUpdate;
        MessageGuid = source.MessageGuid;
        ProviderAnimalId = source.ProviderAnimalId;
        RobotId = source.RobotId;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int AnimalId { get; set; }
    public int TraitId { get; init; }
    public int ProviderId { get; set; }
    public List<OutlierOption> Options { get; set; }
    public DateTime LastUpdate { get; set; }
    public Guid MessageGuid { get; set; }
    public string ProviderAnimalId { get; set; }
    public string Key { get; set; }
    public int RobotId { get; init; }
    public List<(string, Guid)> Guids { get; } = [];

    private string Uid
    {
        get
        {
            DefaultInterpolatedStringHandler h = new DefaultInterpolatedStringHandler(2, 3);
            h.AppendFormatted<int>(this.AnimalId);
            h.AppendLiteral(":");
            h.AppendFormatted<int>(this.TraitId);
            h.AppendLiteral(":");
            h.AppendFormatted<int>(this.ProviderId);
            return h.ToStringAndClear();
        }
    }

    public override int GetHashCode() => this.Uid.GetHashCode();

    public bool Equals(OutlierTrait x, OutlierTrait y)
    {
        if (x == y) return true;
        return x != null && y != null && x.Uid == y.Uid;
    }

    public int GetHashCode(OutlierTrait obj) => obj.Uid.GetHashCode();
}
