using AutoMapper;
using Contracts.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroupUser.Queries
{
    public sealed class GetMachineGroupUserQueryHandlerTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IUserLookup> _mockUserLookup = new(MockBehavior.Loose);

        private GetMachineGroupUserQueryHandler CreateSut()
        {
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync()).ReturnsAsync(new List<DepartmentLookupDto>());
            _mockUserLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserLookupDto>());
            return new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockUserLookup.Object);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var dtos = new List<MachineGroupUserDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllMachineGroupUserAsync(1, 10, null)).ReturnsAsync((dtos, 1));
            _mockMapper.Setup(m => m.Map<List<MachineGroupUserDto>>(It.IsAny<object>())).Returns(dtos);

            var result = await CreateSut().Handle(new GetMachineGroupUserQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo.Setup(r => r.GetAllMachineGroupUserAsync(1, 10, null)).ReturnsAsync((new List<MachineGroupUserDto>(), 0));
            _mockMapper.Setup(m => m.Map<List<MachineGroupUserDto>>(It.IsAny<object>())).Returns(new List<MachineGroupUserDto>());

            var result = await CreateSut().Handle(new GetMachineGroupUserQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
