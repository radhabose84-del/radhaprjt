using Contracts.Common;
using FAM.Application.AuditLog.Queries;
using FAM.Application.AuditLog.Queries.GetAuditLog;
using FAM.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AuditLogControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private AuditLogController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAllAuditLogs_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AuditLogDto>
                {
                    new AuditLogDto { Id = "1", Action = "Create", Module = "Test" }
                });

            var result = await CreateSut().GetAllAuditLogsAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAuditLogs_CallsMediatorSend_Once()
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
        public async Task GetAuditLogSearch_SuccessResult_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AuditLogDto>
                    {
                        new AuditLogDto { Id = "1", Action = "Create" }
                    }
                });

            var result = await CreateSut().GetAuditLog("Create");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuditLogSearch_NotSuccessResult_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = false,
                    Message = "No audit logs found matching the search pattern."
                });

            var result = await CreateSut().GetAuditLog("nonexistent");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAuditLogSearch_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AuditLogDto>>
                {
                    IsSuccess = true,
                    Data = new List<AuditLogDto>()
                });

            await CreateSut().GetAuditLog("test");

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAuditLogBySearchPatternQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
