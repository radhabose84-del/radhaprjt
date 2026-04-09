using AutoMapper;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Queries.GetStockItemBased;

namespace PurchaseManagement.UnitTests.Application.StockItem.Queries
{
    public sealed class GetStockItemBasedQueryHandlerTests
    {
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);
        private readonly Mock<IMrsEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private GetStockItemBasedQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockWarehouseLookup.Object, _mockRepo.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetStockDetails(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<GetStockItemDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetStockItemDto>>(It.IsAny<object>()))
                .Returns(new List<GetStockItemDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStockItemBasedQuery { ItemId = 1, WarehouseId = 1 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryWithCorrectParams()
        {
            _mockRepo
                .Setup(r => r.GetStockDetails(5, 10))
                .ReturnsAsync(new List<GetStockItemDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetStockItemDto>>(It.IsAny<object>()))
                .Returns(new List<GetStockItemDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetStockItemBasedQuery { ItemId = 5, WarehouseId = 10 }, CancellationToken.None);

            _mockRepo.Verify(r => r.GetStockDetails(5, 10), Times.Once);
        }
    }
}
