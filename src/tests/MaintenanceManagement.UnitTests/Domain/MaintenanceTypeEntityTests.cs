using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MaintenanceTypeEntityTests
    {
        [Fact]
        public void MaintenanceType_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceType();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MaintenanceType_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceType();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MaintenanceType_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MaintenanceType)).Should().BeTrue();
        }

        [Fact]
        public void MaintenanceType_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceType
            {
                Id = 1,
                TypeName = "Preventive"
            };

            entity.Id.Should().Be(1);
            entity.TypeName.Should().Be("Preventive");
        }

        [Fact]
        public void MaintenanceType_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MaintenanceType
            {
                TypeName = null
            };

            entity.TypeName.Should().BeNull();
        }
    }
}
