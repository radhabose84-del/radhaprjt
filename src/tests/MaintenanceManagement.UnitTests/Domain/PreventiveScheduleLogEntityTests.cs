using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class PreventiveScheduleLogEntityTests
    {
        [Fact]
        public void PreventiveScheduleLog_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PreventiveScheduleLog)).Should().BeFalse();
        }

        [Fact]
        public void PreventiveScheduleLog_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new PreventiveScheduleLog
            {
                Id = 1,
                PreventiveScheduleId = 10,
                PreventiveScheduleDetailId = 20,
                ActionType = "Create",
                ChangedFields = "Frequency,Status",
                Remarks = "Schedule updated",
                Source = "API",
                IsSuccess = true,
                ErrorMessage = null,
                CreatedBy = 5,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1"
            };
            entity.Id.Should().Be(1);
            entity.PreventiveScheduleId.Should().Be(10);
            entity.PreventiveScheduleDetailId.Should().Be(20);
            entity.ActionType.Should().Be("Create");
            entity.ChangedFields.Should().Be("Frequency,Status");
            entity.Remarks.Should().Be("Schedule updated");
            entity.Source.Should().Be("API");
            entity.IsSuccess.Should().BeTrue();
            entity.ErrorMessage.Should().BeNull();
            entity.CreatedBy.Should().Be(5);
            entity.CreatedDate.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
        }

        [Fact]
        public void PreventiveScheduleLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PreventiveScheduleLog
            {
                PreventiveScheduleId = null,
                PreventiveScheduleDetailId = null,
                Remarks = null,
                Source = null,
                ErrorMessage = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null
            };
            entity.PreventiveScheduleId.Should().BeNull();
            entity.PreventiveScheduleDetailId.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.Source.Should().BeNull();
            entity.ErrorMessage.Should().BeNull();
            entity.CreatedDate.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
        }
    }
}
