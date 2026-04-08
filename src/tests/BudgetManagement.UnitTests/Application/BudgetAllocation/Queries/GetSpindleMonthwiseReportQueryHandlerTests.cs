using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetAllocation.Queries
{
    public sealed class GetSpindleMonthwiseReportQueryHandlerTests
    {
        private readonly Mock<IBudgetAllocationQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);

        private GetSpindleMonthwiseReportQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockUnitLookup.Object, _mockDeptLookup.Object, _mockCurrencyLookup.Object, _mockFyLookup.Object, _mockCostCenterLookup.Object);

        [Fact]
        public async Task Handle_ReturnsData()
        {
            var data = new List<GetSpindleMonthwiseReportDto>();
            _mockRepo.Setup(r => r.GetSpindleDetailsMonthwiseAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<List<GetSpindleMonthwiseReportDto>>(It.IsAny<object>())).Returns(new List<GetSpindleMonthwiseReportDto>());

            var result = await CreateSut().Handle(new GetSpindleMonthwiseReportQuery { FinancialYearId = 1 }, CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
