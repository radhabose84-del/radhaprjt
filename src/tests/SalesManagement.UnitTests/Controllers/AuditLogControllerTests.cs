using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.AuditLog.Queries;
using SalesManagement.Application.AuditLog.Queries.GetAuditLog;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers;

public sealed class AuditLogControllerTests
{
    private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

    private AuditLogController CreateSut() => new(_mockMediator.Object);

    // ── GetAllAuditLogs ──────────────────────────────────────────

    [Fact]
    public async Task GetAllAuditLogs_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLogDto>());

        var result = await CreateSut().GetAllAuditLogsAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllAuditLogs_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLogDto>());

        await CreateSut().GetAllAuditLogsAsync();

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── GetAuditLog (search) ─────────────────────────────────────

    [Fact]
    public async Task GetAuditLogSearch_WhenSuccess_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = true,
                Data = new List<AuditLogDto>()
            });

        var result = await CreateSut().GetAuditLog("test");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAuditLogSearch_WhenNotSuccess_ReturnsOkResult()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = false,
                Message = "No results",
                Data = new List<AuditLogDto>()
            });

        var result = await CreateSut().GetAuditLog("nonexistent");

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAuditLogSearch_CallsMediatorSend_Once()
    {
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
            {
                IsSuccess = true,
                Data = new List<AuditLogDto>()
            });

        await CreateSut().GetAuditLog("search");

        _mockMediator.Verify(
            m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
