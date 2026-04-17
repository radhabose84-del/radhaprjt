using Microsoft.AspNetCore.Http;
using PurchaseManagement.Infrastructure.Services;

namespace PurchaseManagement.UnitTests.Infrastructure.Services
{
    public sealed class AuthTokenHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _mockAccessor = new(MockBehavior.Loose);

        [Fact]
        public void CanInstantiate()
        {
            var handler = new AuthTokenHandler(_mockAccessor.Object);
            handler.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_AddsAuthorizationHeader()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer test-token-123";
            _mockAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var handler = new AuthTokenHandler(_mockAccessor.Object)
            {
                InnerHandler = new TestDelegatingHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("test-token-123");
        }

        [Fact]
        public async Task SendAsync_WithoutToken_DoesNotAddAuthorizationHeader()
        {
            var httpContext = new DefaultHttpContext();
            _mockAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var handler = new AuthTokenHandler(_mockAccessor.Object)
            {
                InnerHandler = new TestDelegatingHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotThrow()
        {
            _mockAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var handler = new AuthTokenHandler(_mockAccessor.Object)
            {
                InnerHandler = new TestDelegatingHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        private class TestDelegatingHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }
    }
}
