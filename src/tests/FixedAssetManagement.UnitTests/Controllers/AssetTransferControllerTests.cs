using Contracts.Common;
using FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.Interfaces.IAssetMaster.IAssetTransferIssue;
using FAM.Presentation.Controllers.AssetMaster;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetTransferControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IAssetTransferQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private AssetTransferController CreateSut() =>
            new(_mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task GetAllAssetTransfers_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AssetTransferQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAssetTransfers_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<AssetTransferQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<AssetTransferQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAssetTransfersByAssetTransferId_ReturnsOkResult()
        {
            var dtos = new List<GetAllTransferDtlDto> { AssetTransferIssueBuilders.ValidDtlDto() };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTransferQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateSut().GetAllAssetTransfersAsync(10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsStatusCode201()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetTransferIssueCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetTransferIssueBuilders.ValidCreateCommand());

            result.Should().BeOfType<ObjectResult>()
                .Which.StatusCode.Should().Be(201);
        }
    }
}
