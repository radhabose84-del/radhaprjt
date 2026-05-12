using Microsoft.Extensions.Options;
using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class ResilienceExtensionsTests
{
    private static IResiliencePipelineProvider CreateProvider()
    {
        var options = new ResilienceOptions();
        ResilienceDependencyInjection.EnsureDefaultProfiles(options);
        return new ResiliencePipelineProvider(Options.Create(options));
    }

    [Fact]
    public async Task ExecuteMongoAsync_ValueTask_RunsOperation()
    {
        var provider = CreateProvider();
        var ran = false;

        await provider.ExecuteMongoAsync(
            ResilienceProfileNames.Standard,
            ct => { ran = true; return ValueTask.CompletedTask; });

        ran.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteMongoAsync_TypedValueTask_ReturnsResult()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteMongoAsync(
            ResilienceProfileNames.Standard,
            ct => ValueTask.FromResult(42));

        result.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteMongoTaskAsync_RunsOperation()
    {
        var provider = CreateProvider();
        var ran = false;

        await provider.ExecuteMongoTaskAsync(
            ResilienceProfileNames.Standard,
            ct => { ran = true; return Task.CompletedTask; });

        ran.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteMongoTaskAsync_TypedTask_ReturnsResult()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteMongoTaskAsync(
            ResilienceProfileNames.Standard,
            ct => Task.FromResult("hello"));

        result.Should().Be("hello");
    }

    [Fact]
    public async Task ExecuteSqlAsync_TypedTask_ReturnsResult()
    {
        var provider = CreateProvider();

        var result = await provider.ExecuteSqlAsync(
            ResilienceProfileNames.Standard,
            ct => Task.FromResult(7));

        result.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteSqlAsync_VoidTask_RunsOperation()
    {
        var provider = CreateProvider();
        var ran = false;

        await provider.ExecuteSqlAsync(
            ResilienceProfileNames.Standard,
            ct => { ran = true; return Task.CompletedTask; });

        ran.Should().BeTrue();
    }

    [Fact]
    public void ExecuteSqlAsync_NullProvider_Throws()
    {
        Action act = () => DapperResilienceExtensions
            .ExecuteSqlAsync(null!, ResilienceProfileNames.Standard, _ => Task.FromResult(1))
            .AsTask().GetAwaiter().GetResult();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecuteMongoAsync_NullOperation_Throws()
    {
        var provider = CreateProvider();
        Action act = () => provider
            .ExecuteMongoAsync(ResilienceProfileNames.Standard, (Func<CancellationToken, ValueTask>)null!)
            .AsTask().GetAwaiter().GetResult();

        act.Should().Throw<ArgumentNullException>();
    }
}
