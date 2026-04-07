using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.MachineGroup.Queries.GetActivityMasterAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.ActivityMaster.Queries
{
    public sealed class GetActivityMasterByIdQueryHandlerTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetActivityMasterByIdQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object, _mockUnitLookup.Object, _mockIpService.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new GetActivityMasterByIdDto());
            _mockMapper.Setup(m => m.Map<GetActivityMasterByIdDto>(It.IsAny<object>())).Returns(new GetActivityMasterByIdDto());

            var result = await CreateSut().Handle(new GetActivityMasterByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetActivityMasterAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IActivityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetActivityMasterAutoCompleteQueryHandler CreateSut() => new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetActivityMasterAutoComplete(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ActivityMaster> { new() });
            _mockMapper.Setup(m => m.Map<List<GetActivityMasterAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<GetActivityMasterAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetActivityMasterAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
