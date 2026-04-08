using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterAutoComplete;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenterById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkCenter.Queries
{
    public sealed class GetWorkCenterByIdQueryHandlerTests
    {
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetWorkCenterByIdQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MaintenanceManagement.Domain.Entities.WorkCenter { Id = 1 });
            _mockMapper.Setup(m => m.Map<WorkCenterDto>(It.IsAny<object>())).Returns(new WorkCenterDto());

            var result = await CreateSut().Handle(new GetWorkCenterByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
    }

    public sealed class GetWorkCenterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetWorkCenterAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            _mockQueryRepo.Setup(r => r.GetWorkCenterGroups(It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.WorkCenter> { new() });
            _mockMapper.Setup(m => m.Map<List<WorkCenterAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<WorkCenterAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetWorkCenterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }
    }
}
