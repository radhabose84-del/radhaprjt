using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ActivityMasterEntityTests
    {
        [Fact]
        public void ActivityMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ActivityMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ActivityMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ActivityMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ActivityMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ActivityMaster)).Should().BeTrue();
        }

        [Fact]
        public void ActivityMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ActivityMaster
            {
                Id = 1,
                ActivityName = "Lubrication",
                Description = "Lubrication activity",
                UnitId = 10,
                DepartmentId = 5,
                EstimatedDuration = 60,
                ActivityType = 2
            };
            entity.Id.Should().Be(1);
            entity.ActivityName.Should().Be("Lubrication");
            entity.Description.Should().Be("Lubrication activity");
            entity.UnitId.Should().Be(10);
            entity.DepartmentId.Should().Be(5);
            entity.EstimatedDuration.Should().Be(60);
            entity.ActivityType.Should().Be(2);
        }

        [Fact]
        public void ActivityMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ActivityMaster
            {
                ActivityName = null,
                Description = null
            };
            entity.ActivityName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
