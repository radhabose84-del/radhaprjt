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
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMasterById;
using ProjectManagement.UnitTests.TestData;

namespace ProjectManagement.UnitTests.Application.ProjectMaster.Queries
{
    public sealed class GetProjectMasterByIdQueryHandlerTests
    {
        private readonly Mock<IProjectMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);
        private readonly Mock<IAssetGroupLookup> _mockAssetGroupLookup = new(MockBehavior.Loose);
        private readonly Mock<ICostCenterLookup> _mockCostCenterLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearLookup> _mockFinancialYearLookup = new(MockBehavior.Loose);

        private GetProjectMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockDeptLookup.Object, _mockUnitLookup.Object,
                _mockMapper.Object, _mockMediator.Object,
                _mockCurrencyLookup.Object, _mockAssetGroupLookup.Object,
                _mockCostCenterLookup.Object, _mockFinancialYearLookup.Object);

        private void SetupLookups()
        {
            _mockDeptLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((DepartmentLookupDto?)null);
            _mockUnitLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UnitLookupDto?)null);
            _mockFinancialYearLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((FinancialYearLookupDto?)null);
            _mockCostCenterLookup.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CostCenterLookupDto?)null);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            var dto = ProjectMasterBuilders.ValidDto();
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dto);
            _mockMapper
                .Setup(m => m.Map<GetProjectMasterDto>(It.IsAny<object>()))
                .Returns(dto);
            SetupLookups();

            var result = await CreateSut().Handle(
                new GetProjectMasterByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetProjectMasterDto?)null);

            Func<Task> act = async () => await CreateSut().Handle(
                new GetProjectMasterByIdQuery { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
