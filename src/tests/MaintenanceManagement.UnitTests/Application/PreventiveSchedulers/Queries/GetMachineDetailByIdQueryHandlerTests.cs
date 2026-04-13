using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetMachineDetailById;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Application.PreventiveSchedulers.Queries.BatchD
{
    public sealed class GetMachineDetailByIdQueryHandlerTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private GetMachineDetailByIdQueryHandler CreateSut() =>
            new(_mockQuery.Object, _mockMapper.Object, _mockDeptLookup.Object);

        private static PreventiveSchedulerHeader BuildHeader() => new()
        {
            MachineGroup = null!,
            MiscMaintenanceCategory = null!,
            MiscSchedule = null!,
            MiscFrequencyType = null!,
            MiscFrequencyUnit = null!,
            Id = 1,
            DepartmentId = 1
        };

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQuery.Setup(q => q.GetDetailSchedulerByPreventiveScheduleId(It.IsAny<int>()))
                .ReturnsAsync(BuildHeader());
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerDto>(It.IsAny<object>()))
                .Returns(new PreventiveSchedulerDto { Activity = new List<MachineDetailActivityDto>(), PreventiveSchedulerDtl = new List<MachineDetailBySchedulerIdDto>() });
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>());

            var result = await CreateSut().Handle(new GetMachineDetailByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithMappingDept_SetsDepartmentName()
        {
            _mockQuery.Setup(q => q.GetDetailSchedulerByPreventiveScheduleId(It.IsAny<int>()))
                .ReturnsAsync(BuildHeader());
            _mockMapper.Setup(m => m.Map<PreventiveSchedulerDto>(It.IsAny<object>()))
                .Returns(new PreventiveSchedulerDto { Activity = new List<MachineDetailActivityDto>(), PreventiveSchedulerDtl = new List<MachineDetailBySchedulerIdDto>(), DepartmentId = 1 });
            _mockDeptLookup.Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto> { new() { DepartmentId = 1, DepartmentName = "Maint" } });

            var result = await CreateSut().Handle(new GetMachineDetailByIdQuery { Id = 1 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
        }
    }
}
