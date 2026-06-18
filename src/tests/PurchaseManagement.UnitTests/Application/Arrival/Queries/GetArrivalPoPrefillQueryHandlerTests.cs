using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalPoPrefill;
using PurchaseManagement.Application.Common.Interfaces.IArrival;

namespace PurchaseManagement.UnitTests.Application.Arrival.Queries
{
    public sealed class GetArrivalPoPrefillQueryHandlerTests
    {
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetArrivalPoPrefillQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        private static IReadOnlyList<ArrivalBalanceQtyDto> SampleBalance() =>
            new List<ArrivalBalanceQtyDto>
            {
                new() { ItemId = 2278, ItemName = "Ring", OrderedQty = 1000m, ArrivedQty = 2m, BalanceQty = 998m }
            };

        private static ApprovedFreightDto SampleFreight() =>
            new()
            {
                FreightRfqId = 7,
                FreightRfqNumber = "FRFQ/2025/0007",
                TransporterId = 101,
                Transporter = "Sri Venkateswara Roadways",
                PartyId = 101,
                Party = "Sri Venkateswara Roadways",
                FreightRate = 4500m
            };

        [Fact]
        public async Task Handle_ReturnsItemsAndApprovedFreight()
        {
            _mockQueryRepo.Setup(r => r.GetBalanceQuantitiesAsync(7)).ReturnsAsync(SampleBalance());
            _mockQueryRepo.Setup(r => r.GetApprovedFreightByPoAsync(7)).ReturnsAsync(SampleFreight());

            var result = await CreateSut().Handle(new GetArrivalPoPrefillQuery(7), CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Items[0].BalanceQty.Should().Be(998m);
            result.ApprovedFreight.Should().NotBeNull();
            result.ApprovedFreight!.FreightRate.Should().Be(4500m);
            result.ApprovedFreight.TransporterId.Should().Be(101);
            result.ApprovedFreight.PartyId.Should().Be(101);
            result.ApprovedFreight.Party.Should().Be("Sri Venkateswara Roadways");
        }

        [Fact]
        public async Task Handle_NoApprovedRfq_ReturnsNullFreight()
        {
            _mockQueryRepo.Setup(r => r.GetBalanceQuantitiesAsync(9)).ReturnsAsync(new List<ArrivalBalanceQtyDto>());
            _mockQueryRepo.Setup(r => r.GetApprovedFreightByPoAsync(9)).ReturnsAsync((ApprovedFreightDto?)null);

            var result = await CreateSut().Handle(new GetArrivalPoPrefillQuery(9), CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.ApprovedFreight.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetBalanceQuantitiesAsync(It.IsAny<int>())).ReturnsAsync(SampleBalance());
            _mockQueryRepo.Setup(r => r.GetApprovedFreightByPoAsync(It.IsAny<int>())).ReturnsAsync(SampleFreight());

            await CreateSut().Handle(new GetArrivalPoPrefillQuery(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
