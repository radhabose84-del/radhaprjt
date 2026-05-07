using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Infrastructure.Resilience;

namespace Shared.Infrastructure.UnitTests.Resilience;

public sealed class HttpRetryProofTests
{
    [Fact]
    public async Task ProvesRetriesActuallyFire_AndCountsAreCorrect()
    {
        var attempts = 0;
        var handler = new TestHandler(_ =>
        {
            attempts++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Debug));

        // Override Critical with shorter base delay so the test runs in <2s while still
        // exercising all 5 retries. UseJitter=false makes the run deterministic.
        services.AddBsoftResilience(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resilience:Profiles:Critical:RetryCount"]            = "5",
                ["Resilience:Profiles:Critical:BaseDelayMs"]           = "1",
                ["Resilience:Profiles:Critical:TimeoutSeconds"]        = "10",
                ["Resilience:Profiles:Critical:BreakerFailureRatio"]   = "0.99",
                ["Resilience:Profiles:Critical:BreakerSamplingSeconds"] = "30",
                ["Resilience:Profiles:Critical:BreakerMinThroughput"]  = "1000",
                ["Resilience:Profiles:Critical:BreakerBreakSeconds"]   = "30",
                ["Resilience:Profiles:Critical:UseJitter"]             = "false",
            })
            .Build());

        services.AddHttpClient("test")
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddBsoftHttpResilience(ResilienceProfileNames.Critical);

        var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("test");

        var response = await client.GetAsync("https://example.invalid/");

        // Critical profile = 5 retries → 6 total attempts (1 initial + 5 retries)
        attempts.Should().Be(6);
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task ProvesRetriesStop_OnceResponseIsSuccess()
    {
        var attempts = 0;
        var handler = new TestHandler(_ =>
        {
            attempts++;
            return new HttpResponseMessage(attempts < 4
                ? HttpStatusCode.ServiceUnavailable
                : HttpStatusCode.OK);
        });

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBsoftResilience(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resilience:Profiles:Critical:RetryCount"]            = "5",
                ["Resilience:Profiles:Critical:BaseDelayMs"]           = "1",
                ["Resilience:Profiles:Critical:TimeoutSeconds"]        = "10",
                ["Resilience:Profiles:Critical:BreakerFailureRatio"]   = "0.99",
                ["Resilience:Profiles:Critical:BreakerSamplingSeconds"] = "30",
                ["Resilience:Profiles:Critical:BreakerMinThroughput"]  = "1000",
                ["Resilience:Profiles:Critical:BreakerBreakSeconds"]   = "30",
                ["Resilience:Profiles:Critical:UseJitter"]             = "false",
            })
            .Build());

        services.AddHttpClient("test")
            .ConfigurePrimaryHttpMessageHandler(() => handler)
            .AddBsoftHttpResilience(ResilienceProfileNames.Critical);

        var client = services.BuildServiceProvider()
            .GetRequiredService<IHttpClientFactory>().CreateClient("test");

        var response = await client.GetAsync("https://example.invalid/");

        attempts.Should().Be(4); // 3 failures + 1 success
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed class TestHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public TestHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_responder(request));
    }
}
