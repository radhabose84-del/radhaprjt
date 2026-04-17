using BudgetManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Http;

namespace BudgetManagement.UnitTests.Application.AuditLog.Services
{
    public sealed class AuthTokenHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Strict);

        private AuthTokenHandler CreateSut()
        {
            var handler = new AuthTokenHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = new FakeInnerHandler()
            };
            return handler;
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_SetsAuthorizationHeader()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer test-token-123";
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("test-token-123");
        }

        [Fact]
        public async Task SendAsync_WithoutBearerToken_DoesNotSetAuthorizationHeader()
        {
            var context = new DefaultHttpContext();
            // No Authorization header set
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNonBearerToken_DoesNotSetAuthorizationHeader()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Basic dXNlcjpwYXNz";
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotSetAuthorizationHeader()
        {
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

            await invoker.SendAsync(request, CancellationToken.None);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_CallsBaseHandler()
        {
            var context = new DefaultHttpContext();
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut();
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

            var response = await invoker.SendAsync(request, CancellationToken.None);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Fake inner handler that returns 200 OK without making a real HTTP call.
        /// </summary>
        private class FakeInnerHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
        }
    }
}
