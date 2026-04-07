using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroupUser.Queries
{
    public sealed class GetMachineGroupUserByIdQueryHandlerTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMachineGroupUserByIdQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new MachineGroupUserDto());
            _mockMapper.Setup(m => m.Map<MachineGroupUserDto>(It.IsAny<object>())).Returns(new MachineGroupUserDto());

            var result = await CreateSut().Handle(new GetMachineGroupUserByIdQuery { Id = 1 }, CancellationToken.None);
            result.Should().NotBeNull();
        }
    }

    public sealed class GetMachineGroupUserAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMachineGroupUserAutoCompleteQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDeptLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            _mockQueryRepo.Setup(r => r.GetMachineGroupUserByName(It.IsAny<string>())).ReturnsAsync(new List<MachineGroupUserAutoCompleteDto> { new() });
            _mockMapper.Setup(m => m.Map<List<MachineGroupUserAutoCompleteDto>>(It.IsAny<object>())).Returns(new List<MachineGroupUserAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(new GetMachineGroupUserAutoCompleteQuery { SearchPattern = "test" }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
