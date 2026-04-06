using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.Power;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class FeederEntityTests
    {
        [Fact]
        public void Feeder_DefaultIsActive_ShouldBeActive()
        {
            var entity = new Feeder();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void Feeder_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Feeder();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Feeder_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Feeder)).Should().BeTrue();
        }

        [Fact]
        public void Feeder_Properties_ShouldBeAssignable()
        {
            var effectiveDate = DateTimeOffset.UtcNow;
            var entity = new Feeder
            {
                Id = 1,
                FeederCode = "F001",
                FeederName = "Main Feeder",
                FeederGroupId = 2,
                FeederTypeId = 3,
                DepartmentId = 4,
                Description = "Primary feeder",
                MultiplicationFactor = 1.5m,
                EffectiveDate = effectiveDate,
                OpeningReading = 100m,
                HighPriority = true,
                Target = 500m,
                UnitId = 5,
                MeterAvailable = true
            };
            entity.Id.Should().Be(1);
            entity.FeederCode.Should().Be("F001");
            entity.FeederName.Should().Be("Main Feeder");
            entity.FeederGroupId.Should().Be(2);
            entity.FeederTypeId.Should().Be(3);
            entity.HighPriority.Should().BeTrue();
            entity.MeterAvailable.Should().BeTrue();
        }

        [Fact]
        public void Feeder_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Feeder
            {
                FeederCode = null,
                FeederName = null,
                Description = null,
                MultiplicationFactor = null,
                OpeningReading = null,
                Target = null,
                MeterTypeId = null,
                ParentFeederId = null
            };
            entity.FeederCode.Should().BeNull();
            entity.MeterTypeId.Should().BeNull();
            entity.ParentFeederId.Should().BeNull();
        }
    }
}
