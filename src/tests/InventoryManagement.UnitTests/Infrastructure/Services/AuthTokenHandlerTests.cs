using Microsoft.AspNetCore.Http;
using InventoryManagement.Infrastructure.Services;
using System.Net;
using System.Net.Http;

namespace InventoryManagement.UnitTests.Infrastructure.Services
{
    public sealed class AuthTokenHandlerTests
    {
        private static AuthTokenHandler CreateSut(IHttpContextAccessor httpContextAccessor)
        {
            var handler = new AuthTokenHandler(httpContextAccessor)
            {
                InnerHandler = new StubInnerHandler()
            };
            return handler;
        }

        private static Mock<IHttpContextAccessor> CreateMockAccessor(string? authorizationHeader)
        {
            var mock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            var httpContext = new DefaultHttpContext();
            if (authorizationHeader != null)
            {
                httpContext.Request.Headers["Authorization"] = authorizationHeader;
            }
            mock.Setup(a => a.HttpContext).Returns(httpContext);
            return mock;
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_SetsAuthorizationHeader()
        {
            var accessor = CreateMockAccessor("Bearer test-jwt-token-123");
            var handler = CreateSut(accessor.Object);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
            var response = await client.SendAsync(request);

            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("test-jwt-token-123");
        }

        [Fact]
        public async Task SendAsync_WithoutAuthorizationHeader_DoesNotSetAuthorizationHeader()
        {
            var accessor = CreateMockAccessor(null);
            var handler = CreateSut(accessor.Object);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
            var response = await client.SendAsync(request);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithEmptyAuthorizationHeader_DoesNotSetAuthorizationHeader()
        {
            var accessor = CreateMockAccessor("");
            var handler = CreateSut(accessor.Object);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
            var response = await client.SendAsync(request);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNonBearerToken_DoesNotSetAuthorizationHeader()
        {
            var accessor = CreateMockAccessor("Basic dXNlcjpwYXNz");
            var handler = CreateSut(accessor.Object);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
            var response = await client.SendAsync(request);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotSetAuthorizationHeader()
        {
            var mock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            mock.Setup(a => a.HttpContext).Returns((HttpContext?)null);
            var handler = CreateSut(mock.Object);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
            var response = await client.SendAsync(request);

            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_AlwaysCallsBaseHandler()
        {
            var accessor = CreateMockAccessor("Bearer token");
            var handler = CreateSut(accessor.Object);
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");
            var response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Stub inner handler that returns 200 OK for all requests.
        /// </summary>
        private class StubInnerHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
