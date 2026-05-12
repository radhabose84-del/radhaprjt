using Microsoft.Extensions.Options;
using Polly;
using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class ResiliencePipelineProviderTests
{
    private static IResiliencePipelineProvider CreateSut()
    {
        var options = new ResilienceOptions();
        ResilienceDependencyInjection.EnsureDefaultProfiles(options);
        return new ResiliencePipelineProvider(Options.Create(options));
    }

    [Fact]
    public void GetMongoPipeline_ReturnsPipeline()
    {
        var sut = CreateSut();
        var pipeline = sut.GetMongoPipeline(ResilienceProfileNames.Standard);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void GetSqlPipeline_ReturnsPipeline()
    {
        var sut = CreateSut();
        var pipeline = sut.GetSqlPipeline(ResilienceProfileNames.Standard);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void GetSignalRPipeline_ReturnsPipeline()
    {
        var sut = CreateSut();
        var pipeline = sut.GetSignalRPipeline(ResilienceProfileNames.FastFail);
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void GetMongoPipeline_CachesByProfileName()
    {
        var sut = CreateSut();
        var first  = sut.GetMongoPipeline(ResilienceProfileNames.Standard);
        var second = sut.GetMongoPipeline(ResilienceProfileNames.Standard);
        ReferenceEquals(first, second).Should().BeTrue();
    }

    [Fact]
    public void GetSqlPipeline_CachesByProfileName()
    {
        var sut = CreateSut();
        var first  = sut.GetSqlPipeline(ResilienceProfileNames.Critical);
        var second = sut.GetSqlPipeline(ResilienceProfileNames.Critical);
        ReferenceEquals(first, second).Should().BeTrue();
    }

    [Fact]
    public void GetProfile_KnownName_ReturnsProfile()
    {
        var sut = CreateSut();
        var profile = sut.GetProfile(ResilienceProfileNames.Critical);
        profile.RetryCount.Should().Be(5);
    }

    [Fact]
    public void GetProfile_UnknownName_Throws()
    {
        var sut = CreateSut();
        Action act = () => sut.GetProfile("DoesNotExist");
        act.Should().Throw<InvalidOperationException>().WithMessage("*not configured*");
    }

    [Fact]
    public void GetProfile_NullOrWhitespace_Throws()
    {
        var sut = CreateSut();
        Action act1 = () => sut.GetProfile(null!);
        Action act2 = () => sut.GetProfile("   ");
        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        Action act = () => new ResiliencePipelineProvider(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetGenericPipeline_BuildsForArbitraryPredicate()
    {
        var sut = CreateSut();
        var pipeline = sut.GetGenericPipeline(
            ResilienceProfileNames.Standard,
            ex => ex is InvalidOperationException);
        pipeline.Should().NotBeNull();
    }
}
