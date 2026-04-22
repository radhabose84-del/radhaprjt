using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ActivityLogEntityTests
    {
        [Fact]
        public void ActivityLog_ShouldNotInheritFromBaseEntity()
        {
            typeof(PurchaseManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ActivityLog)).Should().BeFalse();
        }

        [Fact]
        public void ActivityLog_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ActivityLog
            {
                Id = 10,
                CreatedDate = now,
                EntityName = "PurchaseOrder",
                EntityId = 5,
                Action = "Create",
                PropertyName = "Status",
                OldValue = "Draft",
                NewValue = "Approved",
                CreatedBy = 1,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                Scope = "Header",
                ScopeKey = "PO-001"
            };

            entity.Id.Should().Be(10);
            entity.CreatedDate.Should().Be(now);
            entity.EntityName.Should().Be("PurchaseOrder");
            entity.EntityId.Should().Be(5);
            entity.Action.Should().Be("Create");
            entity.PropertyName.Should().Be("Status");
            entity.OldValue.Should().Be("Draft");
            entity.NewValue.Should().Be("Approved");
            entity.CreatedBy.Should().Be(1);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.Scope.Should().Be("Header");
            entity.ScopeKey.Should().Be("PO-001");
        }

        [Fact]
        public void ActivityLog_DefaultAction_ShouldBeUpdate()
        {
            var entity = new ActivityLog();
            entity.Action.Should().Be("Update");
        }

        [Fact]
        public void ActivityLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ActivityLog
            {
                OldValue = null,
                NewValue = null,
                CreatedBy = null,
                CreatedByName = null,
                CreatedIP = null,
                Scope = null,
                ScopeKey = null
            };

            entity.OldValue.Should().BeNull();
            entity.NewValue.Should().BeNull();
            entity.CreatedBy.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
            entity.CreatedIP.Should().BeNull();
            entity.Scope.Should().BeNull();
            entity.ScopeKey.Should().BeNull();
        }
    }
}
