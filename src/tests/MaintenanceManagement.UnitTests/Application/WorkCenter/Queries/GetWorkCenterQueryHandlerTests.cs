using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.WorkCenter.Queries
{
    public sealed class GetWorkCenterQueryHandlerTests
    {
        private readonly Mock<IWorkCenterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        private GetWorkCenterQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.WorkCenter> { new() };
            var dtos = new List<WorkCenterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllWorkCenterGroupAsync(1, 10, null)).ReturnsAsync((entities, 1));
            _mockMapper.Setup(m => m.Map<List<WorkCenterDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetWorkCenterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllWorkCenterGroupAsync(1, 10, null)).ReturnsAsync((new List<MaintenanceManagement.Domain.Entities.WorkCenter>(), 0));
            _mockMapper.Setup(m => m.Map<List<WorkCenterDto>>(It.IsAny<object>())).Returns(new List<WorkCenterDto>());

            var result = await CreateSut().Handle(new GetWorkCenterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
