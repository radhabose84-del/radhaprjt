using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Shared.Infrastructure.Behaviors;

namespace Shared.Infrastructure.UnitTests.Behaviors;

public sealed class PermissionBehaviorTests
{
    // ── test doubles ────────────────────────────────────────────────────────
    private readonly Mock<IIPAddressService>      _mockIp          = new(MockBehavior.Strict);
    private readonly Mock<IPermissionService>     _mockPermission  = new(MockBehavior.Strict);
    private readonly Mock<IHttpContextAccessor>   _mockHttpContext = new(MockBehavior.Strict);

    private PermissionBehavior<TRequest, TResponse> CreateSut<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        => new(_mockIp.Object, _mockPermission.Object, _mockHttpContext.Object);

    // ── test commands ────────────────────────────────────────────────────────

    /// <summary>Command that opts in to permission checking (CanAdd).</summary>
    private sealed class ProtectedCommand : IRequest<int>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }

    /// <summary>Command that does NOT implement IRequirePermission — free pass.</summary>
    private sealed class UnprotectedCommand : IRequest<int> { }

    private static RequestHandlerDelegate<int> HandlerReturning(int value) =>
        () => Task.FromResult(value);

    // ── helper: set up the X-Menu-Id header ─────────────────────────────────

    private void SetupHeader(string? menuIdValue)
    {
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new Mock<IHeaderDictionary>();
        var mockHttp    = new Mock<HttpContext>();

        if (menuIdValue is not null)
        {
            StringValues val = menuIdValue;
            mockHeaders.Setup(h => h.TryGetValue("X-Menu-Id", out val)).Returns(true);
        }
        else
        {
            StringValues val = default;
            mockHeaders.Setup(h => h.TryGetValue("X-Menu-Id", out val)).Returns(false);
        }

        mockRequest.SetupGet(r => r.Headers).Returns(mockHeaders.Object);
        mockHttp.SetupGet(c => c.Request).Returns(mockRequest.Object);
        _mockHttpContext.SetupGet(a => a.HttpContext).Returns(mockHttp.Object);
    }

    // ── tests: unprotected command ───────────────────────────────────────────

    [Fact]
    public async Task Handle_UnprotectedCommand_PassesThroughWithoutAnyPermissionCheck()
    {
        // No header setup needed — behavior exits before reading headers
        _mockHttpContext.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

        var sut = CreateSut<UnprotectedCommand, int>();
        var result = await sut.Handle(
            new UnprotectedCommand(), HandlerReturning(99), CancellationToken.None);

        result.Should().Be(99);
        _mockIp.VerifyNoOtherCalls();
        _mockPermission.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_UnprotectedCommand_HandlerCalledExactlyOnce()
    {
        _mockHttpContext.SetupGet(a => a.HttpContext).Returns((HttpContext?)null);

        var callCount = 0;
        RequestHandlerDelegate<int> handler = () => { callCount++; return Task.FromResult(1); };

        var sut = CreateSut<UnprotectedCommand, int>();
        await sut.Handle(new UnprotectedCommand(), handler, CancellationToken.None);

        callCount.Should().Be(1);
    }

    // ── tests: protected command — no X-Menu-Id header (bypass) ─────────────

    [Fact]
    public async Task Handle_ProtectedCommand_NoMenuIdHeader_BypassesPermissionCheck()
    {
        SetupHeader(null); // header absent

        var sut = CreateSut<ProtectedCommand, int>();
        var result = await sut.Handle(
            new ProtectedCommand(), HandlerReturning(55), CancellationToken.None);

        result.Should().Be(55);
        _mockIp.VerifyNoOtherCalls();
        _mockPermission.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ProtectedCommand_MenuIdHeaderZero_BypassesPermissionCheck()
    {
        SetupHeader("0"); // header present but value = 0

        var sut = CreateSut<ProtectedCommand, int>();
        var result = await sut.Handle(
            new ProtectedCommand(), HandlerReturning(55), CancellationToken.None);

        result.Should().Be(55);
        _mockIp.VerifyNoOtherCalls();
        _mockPermission.VerifyNoOtherCalls();
    }

    // ── tests: protected command — userId missing ────────────────────────────

    [Fact]
    public async Task Handle_ProtectedCommand_UserIdZero_ThrowsForbiddenException()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(0);

        var sut = CreateSut<ProtectedCommand, int>();
        Func<Task> act = () => sut.Handle(new ProtectedCommand(), HandlerReturning(1), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*Authentication*");
    }

    [Fact]
    public async Task Handle_ProtectedCommand_UserIdZero_HandlerNeverCalled()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(0);
        var callCount = 0;
        RequestHandlerDelegate<int> handler = () => { callCount++; return Task.FromResult(1); };

        var sut = CreateSut<ProtectedCommand, int>();
        try { await sut.Handle(new ProtectedCommand(), handler, CancellationToken.None); }
        catch (ForbiddenException) { /* expected */ }

        callCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ProtectedCommand_NegativeUserId_ThrowsForbiddenException()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(-1);

        var sut = CreateSut<ProtectedCommand, int>();
        Func<Task> act = () => sut.Handle(new ProtectedCommand(), HandlerReturning(1), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ── tests: protected command — permission denied ─────────────────────────

    [Fact]
    public async Task Handle_ProtectedCommand_PermissionDenied_ThrowsForbiddenException()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(5);
        _mockPermission
            .Setup(p => p.HasPermissionAsync(5, 42, PermissionType.CanAdd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut<ProtectedCommand, int>();
        Func<Task> act = () => sut.Handle(new ProtectedCommand(), HandlerReturning(1), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*CanAdd*");
    }

    [Fact]
    public async Task Handle_ProtectedCommand_PermissionDenied_HandlerNeverCalled()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(5);
        _mockPermission
            .Setup(p => p.HasPermissionAsync(5, 42, PermissionType.CanAdd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var callCount = 0;
        RequestHandlerDelegate<int> handler = () => { callCount++; return Task.FromResult(1); };

        var sut = CreateSut<ProtectedCommand, int>();
        try { await sut.Handle(new ProtectedCommand(), handler, CancellationToken.None); }
        catch (ForbiddenException) { /* expected */ }

        callCount.Should().Be(0);
    }

    // ── tests: protected command — permission granted ────────────────────────

    [Fact]
    public async Task Handle_ProtectedCommand_PermissionGranted_ReturnsHandlerResult()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(5);
        _mockPermission
            .Setup(p => p.HasPermissionAsync(5, 42, PermissionType.CanAdd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut<ProtectedCommand, int>();
        var result = await sut.Handle(new ProtectedCommand(), HandlerReturning(77), CancellationToken.None);

        result.Should().Be(77);
    }

    [Fact]
    public async Task Handle_ProtectedCommand_PermissionGranted_HandlerCalledExactlyOnce()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(5);
        _mockPermission
            .Setup(p => p.HasPermissionAsync(5, 42, PermissionType.CanAdd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var callCount = 0;
        RequestHandlerDelegate<int> handler = () => { callCount++; return Task.FromResult(1); };

        var sut = CreateSut<ProtectedCommand, int>();
        await sut.Handle(new ProtectedCommand(), handler, CancellationToken.None);

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ProtectedCommand_PermissionGranted_CallsPermissionServiceWithCorrectArgs()
    {
        SetupHeader("42");
        _mockIp.Setup(s => s.GetUserId()).Returns(7);
        _mockPermission
            .Setup(p => p.HasPermissionAsync(7, 42, PermissionType.CanAdd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CreateSut<ProtectedCommand, int>();
        await sut.Handle(new ProtectedCommand(), HandlerReturning(1), CancellationToken.None);

        _mockPermission.Verify(
            p => p.HasPermissionAsync(7, 42, PermissionType.CanAdd, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
