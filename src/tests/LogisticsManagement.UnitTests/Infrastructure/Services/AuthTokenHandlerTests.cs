using System.Net;
using System.Net.Http.Headers;
using LogisticsManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace LogisticsManagement.UnitTests.Infrastructure.Services
{
    public sealed class AuthTokenHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Strict);

        private AuthTokenHandler CreateSut() => new(_mockHttpContextAccessor.Object);

        /// <summary>
        /// A test inner handler that captures the request for assertion and returns 200 OK.
        /// </summary>
        private sealed class StubInnerHandler : HttpMessageHandler
        {
            public HttpRequestMessage? CapturedRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                CapturedRequest = request;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        private HttpClient CreateHttpClient(AuthTokenHandler handler, StubInnerHandler inner)
        {
            handler.InnerHandler = inner;
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost")
            };
        }

        private DefaultHttpContext BuildHttpContextWithToken(string? authHeader)
        {
            var context = new DefaultHttpContext();
            if (authHeader != null)
            {
                context.Request.Headers["Authorization"] = authHeader;
            }
            return context;
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_SetsAuthorizationHeader()
        {
            var context = BuildHttpContextWithToken("Bearer test-token-123");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            await client.GetAsync("/api/test");

            inner.CapturedRequest.Should().NotBeNull();
            inner.CapturedRequest!.Headers.Authorization.Should().NotBeNull();
            inner.CapturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
            inner.CapturedRequest.Headers.Authorization.Parameter.Should().Be("test-token-123");
        }

        [Fact]
        public async Task SendAsync_WithoutAuthorizationHeader_DoesNotSetAuthorizationHeader()
        {
            var context = BuildHttpContextWithToken(null);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            await client.GetAsync("/api/test");

            inner.CapturedRequest.Should().NotBeNull();
            inner.CapturedRequest!.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithEmptyAuthorizationHeader_DoesNotSetAuthorizationHeader()
        {
            var context = BuildHttpContextWithToken("");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            await client.GetAsync("/api/test");

            inner.CapturedRequest.Should().NotBeNull();
            inner.CapturedRequest!.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNonBearerToken_DoesNotSetAuthorizationHeader()
        {
            var context = BuildHttpContextWithToken("Basic dXNlcjpwYXNz");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            await client.GetAsync("/api/test");

            inner.CapturedRequest.Should().NotBeNull();
            inner.CapturedRequest!.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotSetAuthorizationHeader()
        {
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            await client.GetAsync("/api/test");

            inner.CapturedRequest.Should().NotBeNull();
            inner.CapturedRequest!.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_ReturnsResponseFromInnerHandler()
        {
            var context = BuildHttpContextWithToken("Bearer some-token");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            var response = await client.GetAsync("/api/test");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SendAsync_WithBearerTokenContainingSpaces_ExtractsTokenCorrectly()
        {
            // Bearer token value is everything after "Bearer "
            var context = BuildHttpContextWithToken("Bearer eyJhbGciOiJIUzI1NiJ9.payload.sig");
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var inner = new StubInnerHandler();
            using var client = CreateHttpClient(CreateSut(), inner);

            await client.GetAsync("/api/test");

            inner.CapturedRequest!.Headers.Authorization!.Parameter
                .Should().Be("eyJhbGciOiJIUzI1NiJ9.payload.sig");
        }

        [Fact]
        public void AuthTokenHandler_ShouldInheritFromDelegatingHandler()
        {
            typeof(DelegatingHandler).IsAssignableFrom(typeof(AuthTokenHandler)).Should().BeTrue();
        }
    }
}
