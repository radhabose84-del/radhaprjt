using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Application.ScheduleIII.Queries.GetSubTotals;

namespace FinanceManagement.UnitTests.Application.ScheduleIII.Queries
{
    public sealed class GetSubTotalsQueryHandlerTests
    {
        private readonly Mock<IScheduleIIIQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetSubTotalsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccessWithData()
        {
            var data = new List<ScheduleIIISubTotalDto>
            {
                new() { Id = 1, FormulaName = "Gross Profit" },
                new() { Id = 2, FormulaName = "EBITDA", IncludeOtherIncome = true }
            };
            _mockQueryRepo.Setup(r => r.GetSubTotalsAsync()).ReturnsAsync(data);

            var result = await CreateSut().Handle(new GetSubTotalsQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessEmpty()
        {
            _mockQueryRepo.Setup(r => r.GetSubTotalsAsync()).ReturnsAsync(new List<ScheduleIIISubTotalDto>());

            var result = await CreateSut().Handle(new GetSubTotalsQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
