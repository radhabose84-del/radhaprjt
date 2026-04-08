using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.AuditLog.Queries;
using UserManagement.Application.AuditLog.Queries.GetAuditLog;
using UserManagement.Presentation.Controllers;

namespace UserManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private AuditLogController CreateSut() => new(_mockSender.Object);

        // --- GetAllAuditLogsAsync ---

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
        public async Task GetAllAuditLogsAsync_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>());

            await CreateSut().GetAllAuditLogsAsync();

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --- GetAuditLog (search) ---

        [Fact]
        public async Task GetAuditLog_SuccessfulSearch_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AuditLogDto> { new AuditLogDto() }
                });

            var result = await CreateSut().GetAuditLog("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuditLog_UnsuccessfulSearch_ReturnsOkResult()
        {
            // Controller returns Ok in both success and failure branches
            _mockSender
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
        public async Task GetAuditLog_CallsMediatorSendOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AuditLogDto>()
                });

            await CreateSut().GetAuditLog("test");

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
