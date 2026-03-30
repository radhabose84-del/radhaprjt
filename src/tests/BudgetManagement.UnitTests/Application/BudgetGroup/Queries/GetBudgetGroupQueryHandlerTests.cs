using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Queries.GetBudgetGroup;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using BudgetManagement.UnitTests.TestData;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;

namespace BudgetManagement.UnitTests.Application.BudgetGroup.Queries
{
    public sealed class GetBudgetGroupQueryHandlerTests
    {
        private readonly Mock<IBudgetGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCCLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private GetBudgetGroupQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockDeptLookup.Object,
                _mockUnitLookup.Object, _mockCCLookup.Object, _mockCurrencyLookup.Object);

        private void SetupLookups()
        {
            _mockDeptLookup.Setup(l => l.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new DepartmentLookupDto { DepartmentId = 1, DepartmentName = "IT" }
                });

            _mockUnitLookup.Setup(l => l.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new UnitLookupDto { UnitId = 1, UnitName = "Unit A" }
                });

            _mockCCLookup.Setup(l => l.GetAllCostCentersAsync())
                .ReturnsAsync(new List<CostCenterLookupDto>
                {
                    new CostCenterLookupDto { CostCenterId = 1, CostCenterName = "CC1" }
                });

            _mockCurrencyLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>
                {
                    new CurrencyLookupDto { CurrencyId = 1, Name = "INR" }
                });
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            SetupLookups();
            var items = new List<BudgetGroupListItemDto> { BudgetGroupBuilders.ValidListItemDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<BudgetGroupListFilterDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 1));

            var result = await CreateSut().Handle(
                new GetBudgetGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            SetupLookups();
            var items = new List<BudgetGroupListItemDto> { BudgetGroupBuilders.ValidListItemDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<BudgetGroupListFilterDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((items, 25));

            var result = await CreateSut().Handle(
                new GetBudgetGroupQuery { PageNumber = 2, PageSize = 5 },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(25);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            SetupLookups();
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<BudgetGroupListFilterDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((new List<BudgetGroupListItemDto>(), 0));

            var result = await CreateSut().Handle(
                new GetBudgetGroupQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
