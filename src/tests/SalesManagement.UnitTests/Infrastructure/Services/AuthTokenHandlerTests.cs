using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using SalesManagement.Infrastructure.Services;

namespace SalesManagement.UnitTests.Infrastructure.Services;

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
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        await invoker.SendAsync(request, CancellationToken.None);

        request.Headers.Authorization.Should().NotBeNull();
        request.Headers.Authorization!.Scheme.Should().Be("Bearer");
        request.Headers.Authorization.Parameter.Should().Be("test-token-123");
    }

    [Fact]
    public async Task SendAsync_WithoutAuthorizationHeader_DoesNotSetAuthorizationHeader()
    {
        var context = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

        var handler = CreateSut();
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        await invoker.SendAsync(request, CancellationToken.None);

        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_WithEmptyAuthorizationHeader_DoesNotSetAuthorizationHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "";
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

        var handler = CreateSut();
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        await invoker.SendAsync(request, CancellationToken.None);

        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_WithNullHttpContext_DoesNotSetAuthorizationHeader()
    {
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var handler = CreateSut();
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

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
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        await invoker.SendAsync(request, CancellationToken.None);

        request.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task SendAsync_CallsInnerHandler()
    {
        var context = new DefaultHttpContext();
        _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(context);

        var handler = CreateSut();
        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/test");

        var response = await invoker.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Fake inner handler that returns 200 OK for all requests.
    /// </summary>
    private sealed class FakeInnerHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
