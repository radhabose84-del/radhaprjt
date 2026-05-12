namespace Shared.Infrastructure.Resilience;

public sealed class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public Dictionary<string, ResilienceProfile> Profiles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
