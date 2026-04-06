using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ShiftMasterDetailEntityTests
    {
        [Fact]
        public void ShiftMasterDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ShiftMasterDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ShiftMasterDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ShiftMasterDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ShiftMasterDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ShiftMasterDetail)).Should().BeTrue();
        }

        [Fact]
        public void ShiftMasterDetail_Properties_ShouldBeAssignable()
        {
            var startTime = new TimeOnly(8, 0);
            var endTime = new TimeOnly(16, 0);
            var effectiveDate = new DateOnly(2025, 1, 1);
            var entity = new ShiftMasterDetail
            {
                Id = 1,
                ShiftMasterId = 2,
                UnitId = 3,
                StartTime = startTime,
                EndTime = endTime,
                DurationInHours = 8m,
                BreakDurationInMinutes = 30,
                EffectiveDate = effectiveDate,
                ShiftSupervisorId = 10
            };
            entity.Id.Should().Be(1);
            entity.ShiftMasterId.Should().Be(2);
            entity.UnitId.Should().Be(3);
            entity.StartTime.Should().Be(startTime);
            entity.EndTime.Should().Be(endTime);
            entity.DurationInHours.Should().Be(8m);
            entity.BreakDurationInMinutes.Should().Be(30);
            entity.EffectiveDate.Should().Be(effectiveDate);
            entity.ShiftSupervisorId.Should().Be(10);
        }
    }
}
