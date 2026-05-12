namespace Shared.Infrastructure.Resilience;

public sealed class ResilienceProfile
{
    public int RetryCount { get; set; } = 3;

    public int BaseDelayMs { get; set; } = 1000;

    public int TimeoutSeconds { get; set; } = 30;

    public double BreakerFailureRatio { get; set; } = 0.5;

    public int BreakerSamplingSeconds { get; set; } = 30;

    public int BreakerMinThroughput { get; set; } = 8;

    public int BreakerBreakSeconds { get; set; } = 30;

    public bool UseJitter { get; set; } = true;
}
