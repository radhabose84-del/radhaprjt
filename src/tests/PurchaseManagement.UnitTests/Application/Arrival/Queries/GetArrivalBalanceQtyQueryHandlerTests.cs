using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalBalanceQty;
using PurchaseManagement.Application.Common.Interfaces.IArrival;

namespace PurchaseManagement.UnitTests.Application.Arrival.Queries
{
    public sealed class GetArrivalBalanceQtyQueryHandlerTests
    {
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetArrivalBalanceQtyQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        private static IReadOnlyList<ArrivalBalanceQtyDto> SampleBalance() =>
            new List<ArrivalBalanceQtyDto>
            {
                new() { ItemId = 1, ItemName = "Cotton DCH-32", OrderedQty = 500m, ArrivedQty = 150m, BalanceQty = 350m }
            };

        [Fact]
        public async Task Handle_ReturnsBalancePerItem()
        {
            _mockQueryRepo.Setup(r => r.GetBalanceQuantitiesAsync(7)).ReturnsAsync(SampleBalance());

            var result = await CreateSut().Handle(new GetArrivalBalanceQtyQuery(7), CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].BalanceQty.Should().Be(350m);
            result[0].OrderedQty.Should().Be(500m);
            result[0].ArrivedQty.Should().Be(150m);
        }

        [Fact]
        public async Task Handle_NoPoLines_ReturnsEmpty()
        {
            _mockQueryRepo.Setup(r => r.GetBalanceQuantitiesAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<ArrivalBalanceQtyDto>());

            var result = await CreateSut().Handle(new GetArrivalBalanceQtyQuery(999), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetBalanceQuantitiesAsync(It.IsAny<int>())).ReturnsAsync(SampleBalance());

            await CreateSut().Handle(new GetArrivalBalanceQtyQuery(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
