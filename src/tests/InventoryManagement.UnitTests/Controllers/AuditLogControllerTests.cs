using Contracts.Common;
using InventoryManagement.Application.AuditLog.Queries;
using InventoryManagement.Application.AuditLog.Queries.GetAuditLog;
using InventoryManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private AuditLogController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAllAuditLogsAsync_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            var result = await CreateSut().GetAllAuditLogsAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAuditLogsAsync_CallsSenderOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            await CreateSut().GetAllAuditLogsAsync();

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAuditLog_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Data = new List<AuditLogDto>()
                });

            var result = await CreateSut().GetAuditLog("test");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
