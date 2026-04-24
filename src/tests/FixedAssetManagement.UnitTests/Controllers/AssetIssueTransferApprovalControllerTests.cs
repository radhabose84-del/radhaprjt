using Contracts.Common;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Commands.UpdateAssetTranferIssueApproval;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById;
using FAM.Presentation.Controllers.AssetMaster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetIssueTransferApprovalControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetIssueTransferApproval>> _mockLogger = new(MockBehavior.Loose);

        private AssetIssueTransferApproval CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetTranferIssueApprovalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferIssueApprovalDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferIssueApprovalDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetIssueTransferPendingAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetTranferIssueApprovalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferIssueApprovalDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferIssueApprovalDto>()
                });

            await CreateSut().GetAllAssetIssueTransferPendingAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetTranferIssueApprovalQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAll_WithDateFilters_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetTranferIssueApprovalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferIssueApprovalDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferIssueApprovalDto>()
                });

            var result = await CreateSut().GetAllAssetIssueTransferPendingAsync(
                1, 10, "Internal", DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetTransferIssueByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetTransferIssueByIdDto>
                {
                    new AssetTransferIssueByIdDto { Id = 1, AssetId = 10, AssetCode = "A001", AssetName = "Laptop", AssetValue = 50000m }
                });

            var result = await CreateSut().GetByAssetTransferIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetTransferIssueByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetTransferIssueByIdDto>());

            await CreateSut().GetByAssetTransferIdAsync(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetTransferIssueByIdQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateStatus_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetTranferIssueApprovalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new UpdateAssetTranferIssueApprovalCommand(new List<int> { 1, 2 }, "Approved");
            var result = await CreateSut().UpdateStatus(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateStatus_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateAssetTranferIssueApprovalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new UpdateAssetTranferIssueApprovalCommand(new List<int> { 1 }, "Rejected");
            await CreateSut().UpdateStatus(command);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<UpdateAssetTranferIssueApprovalCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
