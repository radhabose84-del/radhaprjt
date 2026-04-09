using Contracts.Common;
using FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetails;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending;
using FAM.Presentation.Controllers.AssetMaster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FixedAssetManagement.UnitTests.Controllers
{
    public sealed class AssetTransferReceiptControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<AssetTransferReceiptController>> _mockLogger = new(MockBehavior.Loose);

        private AssetTransferReceiptController CreateSut() =>
            new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetAssetTransferReceiptPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetReceiptPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferReceiptPendingDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferReceiptPendingDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAssetTransferReceiptPendingAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAssetTransferReceiptPending_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetReceiptPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetTransferReceiptPendingDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetTransferReceiptPendingDto>()
                });

            await CreateSut().GetAssetTransferReceiptPendingAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAssetReceiptPendingQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAssetTransferReceiptDtlPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetRecieptDtlPendingQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AssetTrasnferReceiptHdrPendingDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow
                });

            var result = await CreateSut().GetAssetTransferReceiptDtlPendingAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAssetTransferReceiptDetails_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetReceiptDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<AssetReceiptDetailsDto>>
                {
                    IsSuccess = true,
                    Data = new List<AssetReceiptDetailsDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAssetTransferReceiptDetails(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetTransferReceiptCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow
                }
            };
            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateAssetTransferReceiptCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new CreateAssetTransferReceiptCommand
            {
                AssetTransferReceiptHdrDto = new AssetTransferReceiptHdrDto
                {
                    AssetTransferId = 1,
                    DocDate = DateTimeOffset.UtcNow
                }
            };
            await CreateSut().CreateAsync(command);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateAssetTransferReceiptCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByAssetTransferReceiptId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAssetReceiptDetailsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AssetReceiptDetailsByIdDto>
                {
                    new AssetReceiptDetailsByIdDto { AssetReceiptId = 1 }
                });

            var result = await CreateSut().GetByAssetTransferReceiptIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
