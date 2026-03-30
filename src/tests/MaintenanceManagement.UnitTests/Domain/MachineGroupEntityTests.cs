using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MachineGroupEntityTests
    {
        [Fact]
        public void MachineGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MachineGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MachineGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MachineGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MachineGroup_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.MachineGroup))
                .Should().BeTrue();
        }

        [Fact]
        public void MachineGroup_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                Id = 1,
                GroupName = "Lathe Group",
                UnitId = 1
            };
            entity.GroupName.Should().Be("Lathe Group");
        }
    }
}
