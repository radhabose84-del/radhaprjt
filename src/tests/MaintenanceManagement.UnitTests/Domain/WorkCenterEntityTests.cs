using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkCenterEntityTests
    {
        [Fact]
        public void WorkCenter_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.WorkCenter();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void WorkCenter_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.WorkCenter();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void WorkCenter_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.WorkCenter))
                .Should().BeTrue();
        }

        [Fact]
        public void WorkCenter_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                Id = 1,
                WorkCenterCode = "WC001",
                WorkCenterName = "Assembly",
                UnitId = 1
            };
            entity.WorkCenterCode.Should().Be("WC001");
        }
    }
}
