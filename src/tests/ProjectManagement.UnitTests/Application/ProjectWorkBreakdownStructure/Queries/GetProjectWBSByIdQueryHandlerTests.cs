using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetById;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetWBSById;
using ProjectManagement.Domain.Events;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Queries
{
    public sealed class GetProjectWBSByIdQueryHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo =
            new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFinancialYearLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDepartmentLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetProjectWorkBreakdownStructureByIdQueryHandler CreateSut() =>
            new(
                _mockQueryRepo.Object,
                _mockCurrencyLookup.Object,
                _mockUnitLookup.Object,
                _mockFinancialYearLookup.Object,
                _mockCostCenterLookup.Object,
                _mockMediator.Object,
                _mockDepartmentLookup.Object
            );

        private void SetupEmptyLookups()
        {
            _mockUnitLookup.Setup(x => x.GetAllUnitAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.UnitLookupDto>());
            _mockDepartmentLookup.Setup(x => x.GetAllDepartmentAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.DepartmentLookupDto>());
            _mockFinancialYearLookup.Setup(x => x.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Users.FinancialYearLookupDto>());
            _mockCostCenterLookup.Setup(x => x.GetAllCostCentersAsync())
                .ReturnsAsync(new List<Contracts.Dtos.Lookups.Maintenance.CostCenterLookupDto>());
            _mockCurrencyLookup.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>)new List<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            SetupEmptyLookups();

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByIdQuery(1),
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((ProjectWorkBreakdownStructureDto?)null);

            var result = await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByIdQuery(99),
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_CallsRepositoryGetByIdOnce()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((ProjectWorkBreakdownStructureDto?)null);

            await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByIdQuery(5),
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(5), Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = ProjectWorkBreakdownStructureBuilders.ValidDto(id: 2);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(dto);
            SetupEmptyLookups();

            await CreateSut().Handle(
                new GetProjectWorkBreakdownStructureByIdQuery(2),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
