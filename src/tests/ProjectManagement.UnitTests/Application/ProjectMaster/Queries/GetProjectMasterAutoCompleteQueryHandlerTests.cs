using AutoMapper;
using Contracts.Dtos.Lookups.FixedAssetManagement;
using Contracts.Dtos.Lookups.Maintenance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.FixedAssetManagement;
using Contracts.Interfaces.Lookups.Maintenance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using ProjectManagement.Application.Common.Interfaces.IProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectMaster.Queries
{
    public sealed class GetProjectMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IProjectMasterQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetProjectMasterAutoCompleteQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFinancialYearLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);
        private readonly Mock<IAssetGroupLookup> _mockAssetGroupLookup = new(MockBehavior.Loose);

        private GetProjectMasterAutoCompleteQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockLogger.Object, _mockMediator.Object, _mockMapper.Object,
                _mockCurrencyLookup.Object, _mockDeptLookup.Object, _mockUnitLookup.Object,
                _mockFinancialYearLookup.Object, _mockCostCenterLookup.Object, _mockAssetGroupLookup.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockRepo
                .Setup(r => r.GetProjectMasterAutoCompleteAsync(null, null, null, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ProjectMasterAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<ProjectMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ProjectMasterAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new GetProjectMasterAutoCompleteQuery { Take = 10 }, CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsResults_WithLookups()
        {
            var items = new List<ProjectMasterAutoCompleteDto> { ProjectMasterBuilders.ValidAutoCompleteDto() };
            _mockRepo
                .Setup(r => r.GetProjectMasterAutoCompleteAsync(null, null, "PRJ", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);
            _mockMapper
                .Setup(m => m.Map<List<ProjectMasterAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<ProjectMasterAutoCompleteDto> { ProjectMasterBuilders.ValidAutoCompleteDto() });
            _mockDeptLookup.Setup(l => l.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(l => l.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            _mockFinancialYearLookup.Setup(l => l.GetAllFinancialYearAsync()).ReturnsAsync(new List<FinancialYearLookupDto>());
            _mockCostCenterLookup.Setup(l => l.GetAllCostCentersAsync()).ReturnsAsync(new List<CostCenterLookupDto>());

            var result = await CreateSut().Handle(
                new GetProjectMasterAutoCompleteQuery { SearchTerm = "PRJ", Take = 10 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
