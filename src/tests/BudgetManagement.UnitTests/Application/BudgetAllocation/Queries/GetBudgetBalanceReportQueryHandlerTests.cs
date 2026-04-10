using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetAllocation.Queries
{
    public sealed class GetBudgetBalanceReportQueryHandlerTests
    {
        private readonly Mock<IBudgetAllocationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);

        private GetBudgetBalanceReportQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockFyLookup.Object);

        [Fact]
        public async Task Handle_ReturnsData()
        {
            var data = new List<BudgetBalanceReportDto> { new() };
            _mockRepo.Setup(r => r.GetBudgetAllocationsAsync(1)).ReturnsAsync(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetBudgetBalanceReportQuery { FinancialYearId = 1 }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
