using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class HttpClientResilienceExtensionsTests
{
    [Fact]
    public void AddBsoftHttpResilience_RegistersClientWithoutThrowing()
    {
        var services = new ServiceCollection();
        services.AddBsoftResilience(new ConfigurationBuilder().Build());
        services.AddHttpClient("test")
            .AddBsoftHttpResilience(ResilienceProfileNames.Standard);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();

        var client = factory.CreateClient("test");
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddBsoftHttpResilience_NullBuilder_Throws()
    {
        Action act = () => HttpClientResilienceExtensions.AddBsoftHttpResilience(null!, ResilienceProfileNames.Standard);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddBsoftHttpResilience_NullProfileName_Throws()
    {
        var services = new ServiceCollection();
        services.AddBsoftResilience(new ConfigurationBuilder().Build());
        var builder = services.AddHttpClient("test");

        Action act = () => builder.AddBsoftHttpResilience(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task AddBsoftHttpResilience_RetriesTransient5xx()
    {
        var attempts = 0;
        var handler = new CountingHandler(req =>
        {
            attempts++;
            return new HttpResponseMessage(attempts < 3 ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK);
        });

        var services = new ServiceCollection();
        services.AddBsoftResilience(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resilience:Profiles:Standard:RetryCount"]            = "3",
                ["Resilience:Profiles:Standard:BaseDelayMs"]           = "1",
                ["Resilience:Profiles:Standard:TimeoutSeconds"]        = "10",
                ["Resilience:Profiles:Standard:BreakerFailureRatio"]   = "0.99",
                ["Resilience:Profiles:Standard:BreakerSamplingSeconds"] = "30",
                ["Resilience:Profiles:Standard:BreakerMinThroughput"]  = "1000",
                ["Resilience:Profiles:Standard:BreakerBreakSeconds"]   = "30",
                ["Resilience:Profiles:Standard:UseJitter"]             = "false",
            })
            .Build());

        services.AddHttpClient("test")
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddBsoftHttpResilience(ResilienceProfileNames.Standard);

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("test");

        var response = await client.GetAsync("https://example.invalid/");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task AddBsoftHttpResilience_DoesNotRetryClientErrors()
    {
        var attempts = 0;
        var handler = new CountingHandler(req =>
        {
            attempts++;
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        });

        var services = new ServiceCollection();
        services.AddBsoftResilience(new ConfigurationBuilder().Build());
        services.AddHttpClient("test")
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddBsoftHttpResilience(ResilienceProfileNames.Standard);

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("test");

        var response = await client.GetAsync("https://example.invalid/");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        attempts.Should().Be(1);
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public CountingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_responder(request));
    }
}
