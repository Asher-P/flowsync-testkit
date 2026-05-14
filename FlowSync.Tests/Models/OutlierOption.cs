using System.Text.Json.Serialization;

namespace FlowSync.Tests.Models;

public record OutlierOption
{
    private sealed class OutlierOptionEqualityComparer : IEqualityComparer<OutlierOption>
    {
        public bool Equals(OutlierOption x, OutlierOption y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return Compare(x.Id, y.Id, "Id") &
                   Compare(x.Name, y.Name, "Name") &
                   Compare(x.Line, y.Line, "Line") &
                   Compare(x.BaseLine, y.BaseLine, "BaseLine") &
                   Compare(x.OptionStatusId, y.OptionStatusId, "OptionStatusId") &
                   Compare(x.CurrentPrice, y.CurrentPrice, "CurrentPrice") &
                   Compare(x.FairPrice, y.FairPrice, "FairPrice") &
                   Compare(x.ProviderId, y.ProviderId, "ProviderId") &
                   Compare(x.IsOutlier, y.IsOutlier, "IsOutlier") &
                   Compare(x.ParticipantId, y.ParticipantId, "ParticipantId") &
                   Compare(x.PlayerId, y.PlayerId, "PlayerId") &
                   Compare(x.PlayerName, y.PlayerName, "PlayerName") &
                   Compare(x.ProviderOptionId, y.ProviderOptionId, "ProviderOptionId") &
                   x.SuspensionReasons.SetEquals(y.SuspensionReasons);
        }

        private static bool Compare<T>(T actual, T expected, string message, IEqualityComparer<T> comparer = default)
        {
            var result = comparer?.Equals(actual, expected) ?? Equals(actual, expected);
            if (actual == null && expected == null) return true;
            if (!result) Console.WriteLine($" {message}, actual = {actual}, expected = {expected}");
            return result;
        }

        public int GetHashCode(OutlierOption obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
            hashCode.Add(obj.Name);
            hashCode.Add(obj.Line);
            hashCode.Add(obj.BaseLine);
            hashCode.Add(obj.OptionStatusId);
            hashCode.Add(obj.CurrentPrice);
            hashCode.Add(obj.FairPrice);
            hashCode.Add(obj.ProviderId);
            hashCode.Add(obj.IsOutlier);
            hashCode.Add(obj.ParticipantId);
            hashCode.Add(obj.PlayerId);
            hashCode.Add(obj.PlayerName);
            hashCode.Add(obj.ProviderOptionId);
            return hashCode.ToHashCode();
        }
    }

    public static IEqualityComparer<OutlierOption> OutlierOptionComparer { get; } = new OutlierOptionEqualityComparer();

    public long Id { get; init; }
    public string Name { get; init; }
    public string Line { get; init; }
    public string BaseLine { get; init; }
    public int OptionStatusId { get; set; }
    public double CurrentPrice { get; set; }
    public double FairPrice { get; set; }
    public int ProviderId { get; set; }
    public bool IsOutlier { get; set; }
    public DateTime LastUpdate { get; set; }
    public DateTime OutlierLastUpdate { get; set; }
    public int? ParticipantId { get; set; }
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string ProviderOptionId { get; set; }
    public List<CleanMeasurement> CleanMeasurements { get; set; }
    public HashSet<string> SuspensionReasons { get; init; } = [];

    [JsonIgnore]
    public string GetKeyByMetadata => $"{Name}-{Line}-{BaseLine}-{ParticipantId}-{PlayerId}";

    public bool AnySuspensionReasonsAndOptionIsAvailable()
    {
        return SuspensionReasons.Any() && OptionStatusId == OptionStatusCode.Available.Value;
    }

    public record OptionStatusCode(int Value, string Name)
    {
        public static OptionStatusCode Unavailable { get; } = new(2, nameof(Unavailable));
        public static OptionStatusCode Available { get; } = new(1, nameof(Available));
        public override string ToString() => Name;
    }
}
