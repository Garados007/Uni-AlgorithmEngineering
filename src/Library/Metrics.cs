namespace Library;

public sealed class Metrics
{
    public sealed class MetricTiming
    {
        public TimeSpan Parsing { get; set; }

        public double ParsingSec => Parsing.TotalSeconds;

        public TimeSpan Solving { get; set; }

        public double SolvingSec => Solving.TotalSeconds;

        public TimeSpan Writing { get; set; }

        public double WritingSec => Writing.TotalSeconds;
    }

    public sealed class MetricSolution
    {
        public string? Solver { get; set; }

        public long Cost { get; set; }
    }

    public Specification? DataFile { get; set; }

    public MetricTiming Timings { get; } = new();

    public MetricSolution Solution { get; } = new();
}
