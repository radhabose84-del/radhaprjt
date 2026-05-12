using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class ResilienceOptionsTests
{
    [Fact]
    public void AddBsoftResilience_BindsAllProfilesFromConfiguration()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resilience:Profiles:Standard:RetryCount"]            = "4",
                ["Resilience:Profiles:Standard:BaseDelayMs"]           = "1500",
                ["Resilience:Profiles:Standard:TimeoutSeconds"]        = "45",
                ["Resilience:Profiles:Standard:BreakerFailureRatio"]   = "0.6",
                ["Resilience:Profiles:Standard:BreakerSamplingSeconds"] = "20",
                ["Resilience:Profiles:Standard:BreakerMinThroughput"]  = "12",
                ["Resilience:Profiles:Standard:BreakerBreakSeconds"]   = "25",
                ["Resilience:Profiles:Standard:UseJitter"]             = "false",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddBsoftResilience(config);
        var provider = services.BuildServiceProvider();

        var bound = provider.GetRequiredService<IOptions<ResilienceOptions>>().Value;
        var standard = bound.Profiles[ResilienceProfileNames.Standard];

        standard.RetryCount.Should().Be(4);
        standard.BaseDelayMs.Should().Be(1500);
        standard.TimeoutSeconds.Should().Be(45);
        standard.BreakerFailureRatio.Should().Be(0.6);
        standard.BreakerSamplingSeconds.Should().Be(20);
        standard.BreakerMinThroughput.Should().Be(12);
        standard.BreakerBreakSeconds.Should().Be(25);
        standard.UseJitter.Should().BeFalse();
    }

    [Fact]
    public void AddBsoftResilience_AppliesDefaultProfilesWhenSectionMissing()
    {
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddBsoftResilience(config);
        var provider = services.BuildServiceProvider();

        var bound = provider.GetRequiredService<IOptions<ResilienceOptions>>().Value;

        bound.Profiles.Should().ContainKey(ResilienceProfileNames.Standard);
        bound.Profiles.Should().ContainKey(ResilienceProfileNames.Critical);
        bound.Profiles.Should().ContainKey(ResilienceProfileNames.FastFail);

        bound.Profiles[ResilienceProfileNames.Standard].RetryCount.Should().Be(3);
        bound.Profiles[ResilienceProfileNames.Critical].RetryCount.Should().Be(5);
        bound.Profiles[ResilienceProfileNames.FastFail].RetryCount.Should().Be(1);
    }

    [Fact]
    public void AddBsoftResilience_PreservesUserProfilesAndAddsMissingDefaults()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resilience:Profiles:Standard:RetryCount"] = "7",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddBsoftResilience(config);
        var provider = services.BuildServiceProvider();

        var bound = provider.GetRequiredService<IOptions<ResilienceOptions>>().Value;

        bound.Profiles[ResilienceProfileNames.Standard].RetryCount.Should().Be(7);
        bound.Profiles[ResilienceProfileNames.Critical].RetryCount.Should().Be(5);
        bound.Profiles[ResilienceProfileNames.FastFail].RetryCount.Should().Be(1);
    }

    [Fact]
    public void EnsureDefaultProfiles_OnEmptyOptions_PopulatesAllThreeProfiles()
    {
        var options = new ResilienceOptions();

        ResilienceDependencyInjection.EnsureDefaultProfiles(options);

        options.Profiles.Should().HaveCount(3);
        options.Profiles[ResilienceProfileNames.Standard].UseJitter.Should().BeTrue();
        options.Profiles[ResilienceProfileNames.FastFail].TimeoutSeconds.Should().Be(5);
    }

    [Fact]
    public void AddBsoftResilience_ThrowsOnNullArguments()
    {
        Action act1 = () => ResilienceDependencyInjection.AddBsoftResilience(null!, new ConfigurationBuilder().Build());
        Action act2 = () => new ServiceCollection().AddBsoftResilience(null!);

        act1.Should().Throw<ArgumentNullException>();
        act2.Should().Throw<ArgumentNullException>();
    }
}
