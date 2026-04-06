using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ShiftMasterEntityTests
    {
        [Fact]
        public void ShiftMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ShiftMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ShiftMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ShiftMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ShiftMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ShiftMaster)).Should().BeTrue();
        }

        [Fact]
        public void ShiftMaster_Properties_ShouldBeAssignable()
        {
            var effectiveDate = new DateOnly(2025, 1, 1);
            var entity = new ShiftMaster
            {
                Id = 1,
                ShiftCode = "S001",
                ShiftName = "Morning Shift",
                EffectiveDate = effectiveDate
            };
            entity.Id.Should().Be(1);
            entity.ShiftCode.Should().Be("S001");
            entity.ShiftName.Should().Be("Morning Shift");
            entity.EffectiveDate.Should().Be(effectiveDate);
        }

        [Fact]
        public void ShiftMaster_NavigationCollection_ShouldAcceptNull()
        {
            var entity = new ShiftMaster { MachineMasters = null };
            entity.MachineMasters.Should().BeNull();
        }
    }
}
