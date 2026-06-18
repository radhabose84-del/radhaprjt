using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotalById;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class GetSubTotalByIdQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSubTotalByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_Found_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetSubTotalByIdAsync(1))
                .ReturnsAsync(new ScheduleIIISubTotalDto { Id = 1, FormulaName = "Gross Profit" });

            var result = await CreateSut().Handle(new GetSubTotalByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.FormulaName.Should().Be("Gross Profit");
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetSubTotalByIdAsync(99)).ReturnsAsync((ScheduleIIISubTotalDto?)null);

            var result = await CreateSut().Handle(new GetSubTotalByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
