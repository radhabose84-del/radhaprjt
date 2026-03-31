using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IProjectWorkBreakdownStructure;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.Dtos;
using ProjectManagement.Application.ProjectWorkBreakdownStructure.Queries.GetAllProjectWBS;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectWorkBreakdownStructure.Queries
{
    public sealed class GetAllProjectWBSQueryHandlerTests
    {
        private readonly Mock<IProjectWorkBreakdownStructureQueryRepository> _mockQueryRepo =
            new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFinancialYearLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDepartmentLookup = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllprojectWBSQueryHandler CreateSut() =>
            new(
                _mockQueryRepo.Object,
                _mockCurrencyLookup.Object,
                _mockUnitLookup.Object,
                _mockFinancialYearLookup.Object,
                _mockCostCenterLookup.Object,
                _mockMapper.Object,
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
            _mockCurrencyLookup.Setup(x => x.GetByIdsAsync(It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>)new List<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessResponse()
        {
            var dtoList = ProjectWorkBreakdownStructureBuilders.ValidDtoList(0);
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, null))
                .ReturnsAsync(((IReadOnlyList<ProjectWorkBreakdownStructureDto>)dtoList, 0));
            SetupEmptyLookups();

            var result = await CreateSut().Handle(
                new GetAllprojectWBSQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = ProjectWorkBreakdownStructureBuilders.ValidDtoList(2);
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "foundation"))
                .ReturnsAsync(((IReadOnlyList<ProjectWorkBreakdownStructureDto>)dtoList, 10));
            SetupEmptyLookups();

            var result = await CreateSut().Handle(
                new GetAllprojectWBSQuery { PageNumber = 2, PageSize = 5, SearchTerm = "foundation" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(10);
        }

        [Fact]
        public async Task Handle_EmptyList_ReturnsSuccessWithNoItems()
        {
            var empty = new List<ProjectWorkBreakdownStructureDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, null))
                .ReturnsAsync(((IReadOnlyList<ProjectWorkBreakdownStructureDto>)empty, 0));
            SetupEmptyLookups();

            var result = await CreateSut().Handle(
                new GetAllprojectWBSQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsRepositoryGetAllOnce()
        {
            var empty = new List<ProjectWorkBreakdownStructureDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, null))
                .ReturnsAsync(((IReadOnlyList<ProjectWorkBreakdownStructureDto>)empty, 0));
            SetupEmptyLookups();

            await CreateSut().Handle(
                new GetAllprojectWBSQuery { PageNumber = 1, PageSize = 20 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, null), Times.Once);
        }
    }
}
