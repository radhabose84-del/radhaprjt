using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Application.AuditLog.Queries;
using ProjectManagement.Application.AuditLog.Queries.GetAuditLog;
using ProjectManagement.Presentation.Controllers;

namespace ProjectManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private AuditLogController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            var result = await CreateSut().GetAllAuditLogsAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
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
        public async Task GetAuditLog_ReturnsOkResult_WhenSuccess()
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
        public async Task GetAuditLog_ReturnsOkResult_WhenNotSuccess()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found.",
                    Data = null
                });

            var result = await CreateSut().GetAuditLog("notexist");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuditLog_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Data = new List<AuditLogDto>()
                });

            await CreateSut().GetAuditLog("pattern");

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
