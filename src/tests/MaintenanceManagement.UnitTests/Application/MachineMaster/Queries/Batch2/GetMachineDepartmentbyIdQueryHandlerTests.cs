using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineDepartmentbyId;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries.Batch2
{
    public sealed class GetMachineDepartmentbyIdQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentGroupLookup> _mockDeptGroupLookup = new(MockBehavior.Loose);

        private GetMachineDepartmentbyIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockDeptGroupLookup.Object);

        [Fact]
        public async Task Handle_WithValidDept_ReturnsMapped()
        {
            var repoDto = new MachineDepartmentGroupDto { DepartmentId = "1", GroupName = "Grp" };
            _mockQueryRepo.Setup(r => r.GetMachineDepartment(1)).ReturnsAsync(repoDto);
            _mockMapper
                .Setup(m => m.Map<MachineDepartmentGroupDto>(It.IsAny<object>()))
                .Returns(new MachineDepartmentGroupDto { DepartmentId = "1", GroupName = "Grp" });

            _mockDeptLookup
                .Setup(d => d.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentLookupDto { DepartmentId = 1, DepartmentName = "HR", Departmentgroupid = 5 });
            _mockDeptGroupLookup
                .Setup(d => d.GetByIdAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DepartmentGroupLookupDto { DepartmentGroupId = 5, DepartmentGroupName = "Admin" });

            var result = await CreateSut().Handle(
                new GetMachineDepartmentbyIdQuery { MachineGroupId = 1 },
                CancellationToken.None);

            result.DepartmentName.Should().Be("HR");
            result.DepartmentGroupName.Should().Be("Admin");
        }

        [Fact]
        public async Task Handle_WithNullRepoResult_StillReturnsMapped()
        {
            _mockQueryRepo.Setup(r => r.GetMachineDepartment(It.IsAny<int>())).ReturnsAsync((MachineDepartmentGroupDto?)null);
            _mockMapper
                .Setup(m => m.Map<MachineDepartmentGroupDto>(It.IsAny<object>()))
                .Returns(new MachineDepartmentGroupDto());

            var result = await CreateSut().Handle(
                new GetMachineDepartmentbyIdQuery { MachineGroupId = 99 },
                CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetMachineDepartment(1)).ReturnsAsync(new MachineDepartmentGroupDto { DepartmentId = "1" });
            _mockMapper
                .Setup(m => m.Map<MachineDepartmentGroupDto>(It.IsAny<object>()))
                .Returns(new MachineDepartmentGroupDto { DepartmentId = "1" });
            _mockDeptLookup.Setup(d => d.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DepartmentLookupDto?)null);

            await CreateSut().Handle(new GetMachineDepartmentbyIdQuery { MachineGroupId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineDepartment"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
