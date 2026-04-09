using Contracts.Common;
using FAM.Application.Reports.AssetAudit;
using FAM.Application.Reports.AssetReport;
using FAM.Application.Reports.AssetTransferReport;
using FAM.Presentation.Controllers.Reports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class ReportControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private ReportController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task AssetReport_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<AssetReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetReportDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AssetReportDto>()
                });

            var result = await CreateSut().AssetReportAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssetReport_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<AssetReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetReportDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetReportDto>()
                });

            await CreateSut().AssetReportAsync();

            _mockSender.Verify(
                m => m.Send(It.IsAny<AssetReportQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AssetTransferReport_WithData_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<AssetTransferQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferDetailsDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AssetTransferDetailsDto>
                    {
                        new AssetTransferDetailsDto { TransferId = 1 }
                    }
                });

            var result = await CreateSut().AssetTransferReportAsync(
                DateTimeOffset.UtcNow.AddMonths(-1), DateTimeOffset.UtcNow);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssetTransferReport_NoData_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<AssetTransferQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferDetailsDto>>
                {
                    IsSuccess = false,
                    Message = "No Asset Transfer Report found.",
                    Data = new List<AssetTransferDetailsDto>()
                });

            var result = await CreateSut().AssetTransferReportAsync(
                DateTimeOffset.UtcNow.AddMonths(-1), DateTimeOffset.UtcNow);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task AssetAuditReport_WithData_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<AssetAuditReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetAuditReportDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = new List<AssetAuditReportDto>
                    {
                        new AssetAuditReportDto { Audit_AssetCode = "AST001" }
                    }
                });

            var result = await CreateSut().AssetAuditReportAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AssetAuditReport_NoData_ReturnsNotFound()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<AssetAuditReportQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetAuditReportDto>>
                {
                    IsSuccess = false,
                    Message = "No Asset Audit Report found.",
                    Data = new List<AssetAuditReportDto>()
                });

            var result = await CreateSut().AssetAuditReportAsync(1);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
