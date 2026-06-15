using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenter;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterAutoComplete;
using MaintenanceManagement.Application.CostCenter.Queries.GetCostCenterById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.CostCenter.Queries
{
    public sealed class GetCostCenterByIdQueryHandlerTests
    {
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetCostCenterByIdQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.CostCenter { Id = 1 });
            _mockMapper.Setup(m => m.Map<CostCenterDto>(It.IsAny<object>())).Returns(new CostCenterDto { Id = 1 });

            var result = await CreateSut().Handle(new GetCostCenterByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull_DoesNotThrow()
        {
            // Repo returns null (missing id / out of unit scope) → handler must return null,
            // NOT dereference a null DTO (regression test for the GetById NullReferenceException 500).
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MaintenanceManagement.Domain.Entities.CostCenter?)null);

            var result = await CreateSut().Handle(new GetCostCenterByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }

    public sealed class GetCostCenterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ICostCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCostCenterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetCostCenterGroups(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.CostCenter> { new() });
            _mockMapper.Setup(m => m.Map<List<CostCenterAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<CostCenterAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetCostCenterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetCostCenterGroups(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.CostCenter>());
            _mockMapper.Setup(m => m.Map<List<CostCenterAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<CostCenterAutoCompleteDto>());

            var result = await CreateSut().Handle(new GetCostCenterAutoCompleteQuery { SearchPattern = "none" }, CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
