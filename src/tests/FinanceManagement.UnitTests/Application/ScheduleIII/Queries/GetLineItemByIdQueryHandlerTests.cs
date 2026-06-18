using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetLineItemById;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class GetLineItemByIdQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetLineItemByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetLineItemByIdAsync(14))
                .ReturnsAsync(new ScheduleIIISectionItemDto { Id = 14, LineCode = "INV", LineName = "Inventories" });

            var result = await CreateSut().Handle(new GetLineItemByIdQuery { Id = 14 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.LineCode.Should().Be("INV");
        }

        [Fact]
        public async Task Handle_MissingId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetLineItemByIdAsync(99)).ReturnsAsync((ScheduleIIISectionItemDto?)null);

            var result = await CreateSut().Handle(new GetLineItemByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
