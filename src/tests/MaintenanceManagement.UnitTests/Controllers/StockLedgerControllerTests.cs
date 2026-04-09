using Contracts.Common;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentAllItemsById;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStock;
using MaintenanceManagement.Application.StockLedger.Queries.GetCurrentStockItemsById;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class StockLedgerControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private StockLedgerController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetCurrentStock_ValidParams_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentStockQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<CurrentStockDto> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetSubStoresCurrentStock("U001", "ITEM001", 1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetCurrentStock_EmptyParams_ReturnsBadRequest()
        {
            var result = await CreateSut().GetSubStoresCurrentStock("", "", 0);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetCurrentStock_NotFound_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentStockQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<CurrentStockDto> { IsSuccess = false, Message = "Not found" });

            var result = await CreateSut().GetSubStoresCurrentStock("U001", "ITEM001", 1);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetStockItemCodes_Success_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentStockItemsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StockItemCodeDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetStockItemCodesAsync("U001", 1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetStockItemCodes_NotFound_ReturnsNotFound()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentStockItemsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StockItemCodeDto>> { IsSuccess = false, Message = "Not found" });

            var result = await CreateSut().GetStockItemCodesAsync("U001", 1);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetAllItemCodes_Success_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetCurrentAllItemsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StockItemCodeDto>> { IsSuccess = true, Data = new() });

            var result = await CreateSut().GetAllItemCodesAsync("U001", 1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
