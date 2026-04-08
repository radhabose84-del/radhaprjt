using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.AuditLog.Queries;
using PurchaseManagement.Application.AuditLog.Queries.GetAuditLog;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Loose);

        private AuditLogController CreateSut() => new(_mockMediator.Object);

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

        [Fact]
        public async Task GetAuditLogSearch_WhenSuccess_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
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
                    Message = "No audit logs found"
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

            await CreateSut().GetAuditLog("test");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
