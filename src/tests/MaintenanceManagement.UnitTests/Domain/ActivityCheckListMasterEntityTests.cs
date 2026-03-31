using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ActivityCheckListMasterEntityTests
    {
        [Fact]
        public void ActivityCheckListMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ActivityCheckListMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ActivityCheckListMaster_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.ActivityCheckListMaster))
                .Should().BeTrue();
        }

        [Fact]
        public void ActivityCheckListMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.ActivityCheckListMaster
            {
                Id = 1,
                ActivityId = 2,
                ActivityCheckList = "Check Oil Level",
                UnitId = 1
            };
            entity.ActivityId.Should().Be(2);
            entity.ActivityCheckList.Should().Be("Check Oil Level");
        }
    }
}
