using AutoMapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;

namespace ProjectManagement.UnitTests.Application.ProjectMaster.Queries
{
    public sealed class GetProjectMasterQueryHandlerTests
    {
        private readonly Mock<IProjectMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IAssetGroupLookup> _mockAssetGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFinancialYearLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);

        private GetProjectMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockCurrencyLookup.Object, _mockAssetGroupLookup.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object,
                _mockFinancialYearLookup.Object, _mockCostCenterLookup.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetProjectmasterAsync(1, 20, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<GetProjectMasterDto>)new List<GetProjectMasterDto>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<GetProjectMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetProjectMasterDto>());

            var result = await CreateSut().Handle(
                new GetProjectMasterQuery { PageNumber = 1, PageSize = 20 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            _mockQueryRepo
                .Setup(r => r.GetProjectmasterAsync(2, 5, "test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<GetProjectMasterDto>)new List<GetProjectMasterDto>(), 11));
            _mockMapper
                .Setup(m => m.Map<List<GetProjectMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetProjectMasterDto>());

            var result = await CreateSut().Handle(
                new GetProjectMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WithLookups()
        {
            var dtos = (IReadOnlyList<GetProjectMasterDto>)new List<GetProjectMasterDto>();
            _mockQueryRepo
                .Setup(r => r.GetProjectmasterAsync(1, 20, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((dtos, 0));
            _mockMapper
                .Setup(m => m.Map<List<GetProjectMasterDto>>(It.IsAny<object>()))
                .Returns(new List<GetProjectMasterDto>());
            _mockDeptLookup.Setup(l => l.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(l => l.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockFinancialYearLookup.Setup(l => l.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>());
            _mockCostCenterLookup.Setup(l => l.GetAllCostCentersAsync()).ReturnsAsync(new List<CostCenterLookupDto>());

            var result = await CreateSut().Handle(
                new GetProjectMasterQuery { PageNumber = 1, PageSize = 20 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
