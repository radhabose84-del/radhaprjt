using MaintenanceManagement.Application.MainStoreStock.Queries.GetItemStockbyId;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStock;
using MaintenanceManagement.Application.MainStoreStock.Queries.GetMainStoreStockItems;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class MainStoreStockControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private MainStoreStockController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetMainStoresCurrentStock_ValidParams_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMainStoreStockQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MainStoresStockDto>());

            var result = await CreateSut().GetMainStoresCurrentStock("U001", "GRP01");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMainStoresCurrentStock_EmptyParams_ReturnsBadRequest()
        {
            var result = await CreateSut().GetMainStoresCurrentStock("", "");
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetMainStoresCurrentStockItems_ValidParams_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetMainStoreStockItemsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MainStoresStockItemsDto>());

            var result = await CreateSut().GetMainStoresCurrentStockItems("U001", "GRP01");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetMainStoresCurrentStockItems_EmptyParams_ReturnsBadRequest()
        {
            var result = await CreateSut().GetMainStoresCurrentStockItems("", "");
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetByItemCodeId_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetItemStockbyIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MainStoreItemStockDto());

            var result = await CreateSut().GetByItemCodeIdAsync("U001", "ITEM001");
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
