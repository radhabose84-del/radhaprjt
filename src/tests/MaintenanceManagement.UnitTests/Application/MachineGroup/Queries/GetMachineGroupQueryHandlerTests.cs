using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroup;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroup.Queries
{
    public sealed class GetMachineGroupQueryHandlerTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetMachineGroupQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUnitLookup.Setup(u => u.GetAllUnitAsync()).ReturnsAsync(new List<UnitLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUnitLookup.Object, _mockIpService.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<MaintenanceManagement.Domain.Entities.MachineGroup> { new() };
            var dtos = new List<MachineGroupDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllMachineGroupsAsync(1, 10, null)).ReturnsAsync((entities, 1));
            _mockMapper.Setup(m => m.Map<List<MachineGroupDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetMachineGroupQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllMachineGroupsAsync(1, 10, null)).ReturnsAsync((new List<MaintenanceManagement.Domain.Entities.MachineGroup>(), 0));
            _mockMapper.Setup(m => m.Map<List<MachineGroupDto>>(It.IsAny<object>())).Returns(new List<MachineGroupDto>());

            var result = await CreateSut().Handle(new GetMachineGroupQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
