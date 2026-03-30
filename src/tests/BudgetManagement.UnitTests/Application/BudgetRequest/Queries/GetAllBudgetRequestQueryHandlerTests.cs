using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Queries.GetAll;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.UnitTests.TestData;
using Contracts.Dtos.Lookups.Projects;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Projects;
using Contracts.Interfaces.Lookups.Users;

namespace BudgetManagement.UnitTests.Application.BudgetRequest.Queries
{
    public sealed class GetAllBudgetRequestQueryHandlerTests
    {
        private readonly Mock<IBudgetRequestQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFyLookup = new(MockBehavior.Loose);
        private readonly Mock<IProjectLookup> _mockProjectLookup = new(MockBehavior.Loose);
        private readonly Mock<IProjectWbsLookup> _mockWbsLookup = new(MockBehavior.Loose);

        private GetBudgetRequestListQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockUnitLookup.Object, _mockCurrencyLookup.Object,
                _mockFyLookup.Object, _mockProjectLookup.Object, _mockWbsLookup.Object);

        private void SetupLookups()
        {
            _mockUnitLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Test Unit", ShortName = "TU" }
                });

            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>
                {
                    new CurrencyLookupDto { CurrencyId = 1, Code = "INR", Name = "Indian Rupee" }
                });

            _mockFyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FinancialYearLookupDto>
                {
                    new FinancialYearLookupDto { FinancialYearId = 1, FinancialYearName = "2025-26" }
                });

            _mockProjectLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectLookupDto>());

            _mockWbsLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectWbsLookupDto>());
        }

        [Fact]
        public async Task Handle_WithItems_ReturnsItems()
        {
            var items = new List<BudgetRequestListItemDto>
            {
                BudgetRequestBuilders.ValidListItemDto(1),
                BudgetRequestBuilders.ValidListItemDto(2)
            };

            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 2));

            SetupLookups();

            var (result, total) = await CreateSut().Handle(
                new GetAllBudgetRequestQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().HaveCount(2);
            total.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyEarly()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<BudgetRequestListItemDto>(), 0));

            var (result, total) = await CreateSut().Handle(
                new GetAllBudgetRequestQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithItems_EnrichesUnitName()
        {
            var item = BudgetRequestBuilders.ValidListItemDto(1);
            item.UnitId = 1;
            item.UnitName = null!;

            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<BudgetRequestListItemDto> { item }, 1));

            _mockUnitLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UnitLookupDto> { new UnitLookupDto { UnitId = 1, ShortName = "TU" } });

            _mockCurrencyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());

            _mockFyLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FinancialYearLookupDto>());

            _mockProjectLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectLookupDto>());

            _mockWbsLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectWbsLookupDto>());

            var (result, _) = await CreateSut().Handle(
                new GetAllBudgetRequestQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result[0].UnitName.Should().Be("TU");
        }

        [Fact]
        public async Task Handle_EmptyResult_DoesNotCallLookups()
        {
            _mockRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<BudgetRequestListItemDto>(), 0));

            await CreateSut().Handle(
                new GetAllBudgetRequestQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockUnitLookup.Verify(
                l => l.GetByIdsAsync(It.IsAny<IReadOnlyList<int>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
