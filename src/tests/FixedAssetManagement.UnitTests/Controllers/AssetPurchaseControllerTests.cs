using Contracts.Common;
using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Presentation.Controllers.AssetPurchase;
using FixedAssetManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetPurchaseControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetPurchaseController>> _mockLogger = new(MockBehavior.Loose);

        private AssetPurchaseController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAllAssetPurchaseDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetPurchaseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetPurchaseDetailsDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetPurchaseDetailsDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAssetPurchaseDetails(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAssetPurchaseDetails_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetPurchaseQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetPurchaseDetailsDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetPurchaseDetailsDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAssetPurchaseDetails(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAssetPurchaseQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetPurchaseDetailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(AssetPurchaseBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetPurchaseDetailCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(AssetPurchaseBuilders.ValidCreateCommand());

            _mockMediator.Verify(m => m.Send(It.IsAny<CreateAssetPurchaseDetailCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
