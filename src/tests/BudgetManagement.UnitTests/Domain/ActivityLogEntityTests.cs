using BudgetManagement.Domain.Entities;

namespace BudgetManagement.UnitTests.Domain
{
    public class ActivityLogEntityTests
    {
        [Fact]
        public void ActivityLog_DefaultAction_ShouldBeUpdate()
        {
            var entity = new ActivityLog();
            entity.Action.Should().Be("Update");
        }

        [Fact]
        public void ActivityLog_DoesNotInheritFromBaseEntity()
        {
            typeof(BudgetManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ActivityLog))
                .Should().BeFalse();
        }

        [Fact]
        public void ActivityLog_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ActivityLog
            {
                Id = 100,
                CreatedDate = now,
                EntityName = "BudgetGroup",
                EntityId = 42,
                Action = "Create",
                PropertyName = "Name",
                OldValue = "Old",
                NewValue = "New",
                CreatedBy = 1,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                Scope = "Budget",
                ScopeKey = "BG-001"
            };

            entity.Id.Should().Be(100);
            entity.CreatedDate.Should().Be(now);
            entity.EntityName.Should().Be("BudgetGroup");
            entity.EntityId.Should().Be(42);
            entity.Action.Should().Be("Create");
            entity.PropertyName.Should().Be("Name");
            entity.OldValue.Should().Be("Old");
            entity.NewValue.Should().Be("New");
            entity.CreatedBy.Should().Be(1);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.Scope.Should().Be("Budget");
            entity.ScopeKey.Should().Be("BG-001");
        }

        [Fact]
        public void ActivityLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ActivityLog
            {
                EntityName = "Test",
                PropertyName = "Prop",
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
