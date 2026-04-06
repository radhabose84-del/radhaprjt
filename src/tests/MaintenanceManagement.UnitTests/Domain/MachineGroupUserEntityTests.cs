using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MachineGroupUserEntityTests
    {
        [Fact]
        public void MachineGroupUser_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MachineGroupUser();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MachineGroupUser_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MachineGroupUser();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MachineGroupUser_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MachineGroupUser)).Should().BeTrue();
        }

        [Fact]
        public void MachineGroupUser_Properties_ShouldBeAssignable()
        {
            var entity = new MachineGroupUser
            {
                Id = 1,
                MachineGroupId = 2,
                DepartmentId = 3,
                UserId = 4
            };
            entity.Id.Should().Be(1);
            entity.MachineGroupId.Should().Be(2);
            entity.DepartmentId.Should().Be(3);
            entity.UserId.Should().Be(4);
        }

        [Fact]
        public void MachineGroupUser_NavigationProperty_ShouldAcceptNull()
        {
            var entity = new MachineGroupUser { MachineGroup = null };
            entity.MachineGroup.Should().BeNull();
        }
    }
}
