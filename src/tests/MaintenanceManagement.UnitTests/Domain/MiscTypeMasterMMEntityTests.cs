using Xunit;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class MiscTypeMasterMMEntityTests
    {
        [Fact]
        public void MiscTypeMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscTypeMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscTypeMaster_ShouldInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(MaintenanceManagement.Domain.Entities.MiscTypeMaster))
                .Should().BeTrue();
        }

        [Fact]
        public void MiscTypeMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 1,
                MiscTypeCode = "MT001",
                Description = "Test Type"
            };
            entity.MiscTypeCode.Should().Be("MT001");
        }
    }
}
