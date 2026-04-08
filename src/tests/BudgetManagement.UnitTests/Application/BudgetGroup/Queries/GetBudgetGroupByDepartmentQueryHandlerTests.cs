using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupByDepartment;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Queries
{
    public sealed class GetBudgetGroupByDepartmentQueryHandlerTests
    {
        private readonly Mock<IBudgetGroupQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private GetBudgetGroupByDepartmentQueryHandler CreateSut() => new(_mockRepo.Object);

        [Fact]
        public async Task Handle_ReturnsGroups()
        {
            var data = new List<BudgetGroupAutoCompleteDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetBudgetGroupByDepartmentAsync(1, "test", It.IsAny<CancellationToken>())).ReturnsAsync(data);

            var result = await CreateSut().Handle(new GetBudgetGroupByDepartmentQuery(1, "test"), CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
