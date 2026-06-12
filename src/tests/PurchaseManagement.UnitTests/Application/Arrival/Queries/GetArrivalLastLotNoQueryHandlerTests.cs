using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalLastLotNo;
using PurchaseManagement.Application.Common.Interfaces.IArrival;

namespace PurchaseManagement.UnitTests.Application.Arrival.Queries
{
    public sealed class GetArrivalLastLotNoQueryHandlerTests
    {
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetArrivalLastLotNoQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLastLotNo()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastLotNoAsync())
                .ReturnsAsync(new ArrivalLastLotNoDto { LotNo = 42, ArrivalNumber = "ARV-2026-0042" });

            var result = await CreateSut().Handle(new GetArrivalLastLotNoQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result!.LotNo.Should().Be(42);
            result.ArrivalNumber.Should().Be("ARV-2026-0042");
        }

        [Fact]
        public async Task Handle_NoArrivals_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetLastLotNoAsync()).ReturnsAsync((ArrivalLastLotNoDto?)null);

            var result = await CreateSut().Handle(new GetArrivalLastLotNoQuery(), CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetLastLotNoAsync())
                .ReturnsAsync(new ArrivalLastLotNoDto { LotNo = 1, ArrivalNumber = "ARV-2026-0001" });

            await CreateSut().Handle(new GetArrivalLastLotNoQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
