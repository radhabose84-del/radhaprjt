using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroupAutoComplete;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.UnitTests.TestData;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Queries
{
    public sealed class GetBudgetGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetBudgetGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var dtos = new List<BudgetGroupAutoCompleteDto> { BudgetGroupBuilders.ValidAutoCompleteDto() };
            _mockQueryRepo
                .Setup(r => r.GetBudgetGroupAutoCompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateSut().Handle(
                new GetBudgetGroupAutoCompleteQuery { SearchPattern = "test" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullSearchPattern_UsesEmptyString()
        {
            var dtos = new List<BudgetGroupAutoCompleteDto> { BudgetGroupBuilders.ValidAutoCompleteDto() };
            _mockQueryRepo
                .Setup(r => r.GetBudgetGroupAutoCompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await CreateSut().Handle(
                new GetBudgetGroupAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetBudgetGroupAutoCompleteAsync("xyz", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BudgetGroupAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetBudgetGroupAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
