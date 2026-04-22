using FinanceManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace FinanceManagement.UnitTests.Infrastructure.Services
{
    public sealed class AuthTokenHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Loose);

        private AuthTokenHandler CreateSut()
        {
            var handler = new AuthTokenHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = new TestInnerHandler()
            };
            return handler;
        }

        private HttpRequestMessage CreateRequest() =>
            new(HttpMethod.Get, "https://api.example.com/test");

        private void SetupHttpContextWithToken(string token)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = token;
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns(httpContext);
        }

        [Fact]
        public void Constructor_WithValidDependency_CreatesHandler()
        {
            var handler = new AuthTokenHandler(_mockHttpContextAccessor.Object);
            handler.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_PropagatesTokenToRequest()
        {
            SetupHttpContextWithToken("Bearer my-test-token-123");

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest();

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("my-test-token-123");
        }

        [Fact]
        public async Task SendAsync_WithoutAuthorizationHeader_DoesNotSetAuthorization()
        {
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns(httpContext);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest();

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNonBearerToken_DoesNotSetAuthorization()
        {
            SetupHttpContextWithToken("Basic dXNlcjpwYXNz");

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest();

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotSetAuthorization()
        {
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns((HttpContext?)null);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest();

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithEmptyAuthorizationHeader_DoesNotSetAuthorization()
        {
            SetupHttpContextWithToken("");

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = CreateRequest();

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_AlwaysCallsBaseHandler()
        {
            SetupHttpContextWithToken("Bearer some-token");

            var innerHandler = new TestInnerHandler();
            var handler = new AuthTokenHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = innerHandler
            };
            var invoker = new HttpMessageInvoker(handler);

            await invoker.SendAsync(CreateRequest(), CancellationToken.None);

            innerHandler.WasCalled.Should().BeTrue();
        }

        /// <summary>
        /// A simple inner handler that records whether SendAsync was called.
        /// </summary>
        private sealed class TestInnerHandler : DelegatingHandler
        {
            public bool WasCalled { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                WasCalled = true;
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }
    }
}
