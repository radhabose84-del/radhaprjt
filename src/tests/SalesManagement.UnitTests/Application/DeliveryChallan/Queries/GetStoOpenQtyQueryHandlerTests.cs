using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Application.DeliveryChallan.Queries.GetStoOpenQty;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Queries
{
    public sealed class GetStoOpenQtyQueryHandlerTests
    {
        private readonly Mock<IDeliveryChallanQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStoOpenQtyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingDetail_ReturnsDto()
        {
            var dto = new StoOpenQtyDto();
            _mockQueryRepo.Setup(r => r.GetStoOpenQtyAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(
                new GetStoOpenQtyQuery { StoDetailId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NonExistentDetail_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetStoOpenQtyAsync(99)).ReturnsAsync((StoOpenQtyDto?)null);

            var result = await CreateSut().Handle(
                new GetStoOpenQtyQuery { StoDetailId = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
