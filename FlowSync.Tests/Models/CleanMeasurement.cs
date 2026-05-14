namespace FlowSync.Tests.Models;

public record CleanMeasurement
{
    public string Method { get; init; }
    public double Value { get; init; }
    public double Probability { get; init; }

    public CleanMeasurement() { }

    public CleanMeasurement(string method, double value)
    {
        Method = method;
        Value = value;
        Probability = 1.0 / value;
    }
}
