using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.BudgetRequest.Queries.GetBudgetRequestPending;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Queries
{
    public sealed class GetBudgetRequestPendingQueryHandlerTests
    {
        private readonly Mock<IBudgetRequestQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);

        private GetBudgetRequestPendingQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMediator.Object, _mockUserLookup.Object, _mockIpService.Object, _mockUnitLookup.Object, _mockCurrencyLookup.Object, _mockFyLookup.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockIpService.Setup(i => i.GetUserId()).Returns(1);
            var data = new List<GetBudgetRequestPendingDto>();
            _mockRepo.Setup(r => r.GetPendingRequestAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((data, 0));
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var (items, total) = await CreateSut().Handle(new GetBudgetRequestPendingQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            items.Should().BeEmpty();
            total.Should().Be(0);
        }
    }
}
