using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using InventoryManagement.Infrastructure.Services;

namespace PartyManagement.UnitTests.Application.Infrastructure
{
    public sealed class AuthTokenHandlerTests
    {
        private static AuthTokenHandler CreateSut(IHttpContextAccessor accessor)
        {
            var handler = new AuthTokenHandler(accessor);
            // Set inner handler for testing
            handler.InnerHandler = new TestInnerHandler();
            return handler;
        }

        private static HttpMessageInvoker CreateInvoker(AuthTokenHandler handler) => new(handler);

        [Fact]
        public async Task SendAsync_WithBearerToken_SetsAuthorizationHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Bearer test-token-123";
            var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            accessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut(accessor.Object);
            var invoker = CreateInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("test-token-123");
        }

        [Fact]
        public async Task SendAsync_WithoutAuthorizationHeader_DoesNotSetAuthHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            accessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut(accessor.Object);
            var invoker = CreateInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNonBearerToken_DoesNotSetAuthHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = "Basic abc123";
            var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            accessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut(accessor.Object);
            var invoker = CreateInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNullHttpContext_DoesNotSetAuthHeader()
        {
            // Arrange
            var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            accessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

            var handler = CreateSut(accessor.Object);
            var invoker = CreateInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

            // Act
            await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_Always_CallsBaseHandler()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var accessor = new Mock<IHttpContextAccessor>(MockBehavior.Loose);
            accessor.Setup(a => a.HttpContext).Returns(context);

            var handler = CreateSut(accessor.Object);
            var invoker = CreateInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

            // Act
            var response = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Test inner handler that returns OK for all requests.
        /// </summary>
        private class TestInnerHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
