using BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.UnitTests.Application.BudgetAllocation.Queries
{
    public sealed class GetRemainingBalanceQueryHandlerTests
    {
        private readonly Mock<IBudgetAllocationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetRemainingBalanceQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetRemainingBalanceQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockFyLookup.Object, _mockLogger.Object);

        private static RemainingBalanceWithPrevDto DefaultDto() =>
            new RemainingBalanceWithPrevDto
            {
                BudgetGroupId = 1,
                CurrentRemainingBalance = 40000m,
                PreviousRemainingBalance = 50000m
            };

        [Fact]
        public async Task Handle_WithFinancialYearId_ReturnsBalance()
        {
            _mockRepo
                .Setup(r => r.GetRemainingBalanceAsync(
                    It.IsAny<int>(), It.IsAny<DateOnly?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DefaultDto());

            var query = new GetRemainingBalanceQuery
            {
                BudgetGroupId = 1,
                FinancialYearId = 1,
                BudgetDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.CurrentRemainingBalance.Should().Be(40000m);
        }

        [Fact]
        public async Task Handle_WithoutFinancialYearId_ResolvesFromLookup()
        {
            var fy = new FinancialYearLookupDto
            {
                FinancialYearId = 5,
                StartDate = DateTime.Today.AddMonths(-3),
                EndDate = DateTime.Today.AddMonths(9)
            };

            _mockFyLookup
                .Setup(l => l.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto> { fy });

            _mockRepo
                .Setup(r => r.GetRemainingBalanceAsync(
                    It.IsAny<int>(), It.IsAny<DateOnly?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(),
                    It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(DefaultDto());

            var query = new GetRemainingBalanceQuery
            {
                BudgetGroupId = 1,
                FinancialYearId = 0,
                BudgetDate = DateOnly.FromDateTime(DateTime.Today)
            };

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NoMatchingFinancialYear_ThrowsApplicationException()
        {
            _mockFyLookup
                .Setup(l => l.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto>());

            var query = new GetRemainingBalanceQuery
            {
                BudgetGroupId = 1,
                FinancialYearId = 0,
                BudgetDate = DateOnly.FromDateTime(new DateTime(2020, 1, 1))
            };

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<ApplicationException>();
        }
    }
}
