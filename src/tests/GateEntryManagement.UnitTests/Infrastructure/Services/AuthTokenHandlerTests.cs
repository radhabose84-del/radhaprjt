using GateEntryManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;

namespace GateEntryManagement.UnitTests.Infrastructure.Services
{
    public sealed class AuthTokenHandlerTests
    {
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new(MockBehavior.Loose);

        private AuthTokenHandler CreateSut()
        {
            var handler = new AuthTokenHandler(_mockHttpContextAccessor.Object)
            {
                InnerHandler = new FakeInnerHandler()
            };
            return handler;
        }

        private void SetupHttpContextWithToken(string authorizationHeader)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = authorizationHeader;
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns(httpContext);
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_PropagatesTokenToRequest()
        {
            // Arrange
            SetupHttpContextWithToken("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test");
            var handler = CreateSut();
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Authorization.Should().NotBeNull();
            request.Headers.Authorization!.Scheme.Should().Be("Bearer");
            request.Headers.Authorization.Parameter.Should().Be("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test");
        }

        [Fact]
        public async Task SendAsync_WithoutToken_DoesNotSetAuthorizationHeader()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns(httpContext);

            var handler = CreateSut();
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithEmptyAuthorizationHeader_DoesNotSetToken()
        {
            // Arrange
            SetupHttpContextWithToken("");
            var handler = CreateSut();
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithNonBearerToken_DoesNotSetToken()
        {
            // Arrange
            SetupHttpContextWithToken("Basic dXNlcjpwYXNz");
            var handler = CreateSut();
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_NullHttpContext_DoesNotSetToken()
        {
            // Arrange
            _mockHttpContextAccessor
                .Setup(a => a.HttpContext)
                .Returns((HttpContext?)null);

            var handler = CreateSut();
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            await client.SendAsync(request);

            // Assert
            request.Headers.Authorization.Should().BeNull();
        }

        [Fact]
        public async Task SendAsync_WithBearerToken_CallsInnerHandler()
        {
            // Arrange
            SetupHttpContextWithToken("Bearer test-token");
            var handler = CreateSut();
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };

            // Act
            var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Fake inner handler that returns 200 OK for all requests.
        /// </summary>
        private class FakeInnerHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}
