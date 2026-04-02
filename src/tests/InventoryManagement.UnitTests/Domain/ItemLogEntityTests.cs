using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemLogEntityTests
    {
        [Fact]
        public void ItemLog_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new ItemLog
            {
                Id = 1L,
                CreatedDate = now,
                EntityName = "ItemMaster",
                EntityId = 42,
                Action = "Update",
                PropertyName = "ItemName",
                OldValue = "OldName",
                NewValue = "NewName",
                CreatedBy = 5,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                CorrelationId = "corr-123"
            };

            entity.Id.Should().Be(1L);
            entity.EntityName.Should().Be("ItemMaster");
            entity.EntityId.Should().Be(42);
            entity.Action.Should().Be("Update");
            entity.PropertyName.Should().Be("ItemName");
            entity.OldValue.Should().Be("OldName");
            entity.NewValue.Should().Be("NewName");
            entity.CreatedBy.Should().Be(5);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.CorrelationId.Should().Be("corr-123");
        }

        [Fact]
        public void ItemLog_DefaultAction_ShouldBeUpdate()
        {
            var entity = new ItemLog();
            entity.Action.Should().Be("Update");
        }

        [Fact]
        public void ItemLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemLog
            {
                OldValue = null,
                NewValue = null,
                CreatedBy = null,
                CreatedByName = null,
                CreatedIP = null,
                CorrelationId = null
            };

            entity.OldValue.Should().BeNull();
            entity.NewValue.Should().BeNull();
            entity.CreatedBy.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
            entity.CorrelationId.Should().BeNull();
        }
    }
}
