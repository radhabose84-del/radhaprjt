using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities.Power;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class GeneratorConsumptionEntityTests
    {
        [Fact]
        public void GeneratorConsumption_DefaultIsActive_ShouldBeActive()
        {
            var entity = new GeneratorConsumption();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void GeneratorConsumption_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new GeneratorConsumption();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void GeneratorConsumption_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GeneratorConsumption)).Should().BeTrue();
        }

        [Fact]
        public void GeneratorConsumption_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new GeneratorConsumption
            {
                Id = 1,
                GeneratorId = 2,
                StartTime = now,
                EndTime = now.AddHours(8),
                RunningHours = 8m,
                DieselConsumption = 50m,
                OpeningEnergyReading = 1000m,
                ClosingEnergyReading = 1200m,
                Energy = 200m,
                UnitId = 3
            };
            entity.Id.Should().Be(1);
            entity.GeneratorId.Should().Be(2);
            entity.RunningHours.Should().Be(8m);
            entity.DieselConsumption.Should().Be(50m);
            entity.Energy.Should().Be(200m);
            entity.UnitId.Should().Be(3);
        }

        [Fact]
        public void GeneratorConsumption_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GeneratorConsumption
            {
                PurposeId = null,
                GeneratorTran = null,
                GensetPurposeType = null
            };
            entity.PurposeId.Should().BeNull();
            entity.GeneratorTran.Should().BeNull();
            entity.GensetPurposeType.Should().BeNull();
        }
    }
}
