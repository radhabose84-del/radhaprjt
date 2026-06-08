using MediatR;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalById;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.Arrival.Queries
{
    public sealed class GetArrivalByIdQueryHandlerTests
    {
        private readonly Mock<IArrivalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetArrivalByIdQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ArrivalBuilders.ValidDto());

            var result = await CreateSut().Handle(new GetArrivalByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ArrivalDto?)null);

            var result = await CreateSut().Handle(new GetArrivalByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
