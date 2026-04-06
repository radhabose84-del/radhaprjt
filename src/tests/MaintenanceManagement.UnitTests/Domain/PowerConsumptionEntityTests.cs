using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.Power;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class PowerConsumptionEntityTests
    {
        [Fact]
        public void PowerConsumption_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PowerConsumption();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PowerConsumption_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PowerConsumption();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PowerConsumption_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PowerConsumption)).Should().BeTrue();
        }

        [Fact]
        public void PowerConsumption_Properties_ShouldBeAssignable()
        {
            var entity = new PowerConsumption
            {
                Id = 1,
                FeederTypeId = 2,
                FeederId = 3,
                UnitId = 4,
                OpeningReading = 100m,
                ClosingReading = 250m,
                TotalUnits = 150m
            };
            entity.Id.Should().Be(1);
            entity.FeederTypeId.Should().Be(2);
            entity.FeederId.Should().Be(3);
            entity.UnitId.Should().Be(4);
            entity.OpeningReading.Should().Be(100m);
            entity.ClosingReading.Should().Be(250m);
            entity.TotalUnits.Should().Be(150m);
        }

        [Fact]
        public void PowerConsumption_NavigationProperties_ShouldAcceptNull()
        {
            var entity = new PowerConsumption
            {
                FeederTypePower = null,
                FeederPower = null
            };
            entity.FeederTypePower.Should().BeNull();
            entity.FeederPower.Should().BeNull();
        }
    }
}
