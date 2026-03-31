using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MachineMasterEntityTests
    {
        [Fact]
        public void MachineMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MachineMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MachineMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MachineMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MachineMaster_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.MachineMaster))
                .Should().BeTrue();
        }

        [Fact]
        public void MachineMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MachineMaster
            {
                Id = 1,
                MachineCode = "MCH001",
                MachineName = "Lathe Machine",
                MachineGroupId = 1
            };
            entity.MachineCode.Should().Be("MCH001");
            entity.MachineName.Should().Be("Lathe Machine");
        }
    }
}
