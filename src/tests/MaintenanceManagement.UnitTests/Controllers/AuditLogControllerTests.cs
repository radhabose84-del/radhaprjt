using Contracts.Common;
using MaintenanceManagement.Application.AuditLog.Queries;
using MaintenanceManagement.Application.AuditLog.Queries.GetAuditLog;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AuditLogController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            var result = await CreateSut().GetAllAuditLogsAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuditLogSearch_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetAuditLog("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuditLogSearch_NotSuccess_ReturnsOkWithMessage()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>> { IsSuccess = false, Message = "Not found", Data = new() });

            var result = await CreateSut().GetAuditLog("none");
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
