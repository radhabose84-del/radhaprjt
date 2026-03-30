using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.PutAway;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class PutawayRuleEntityTests
    {
        [Fact]
        public void PutAwayRule_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PutAwayRule();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PutAwayRule_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PutAwayRule();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PutAwayRule_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PutAwayRule)).Should().BeTrue();
        }

        [Fact]
        public void PutAwayRule_Properties_ShouldBeAssignable()
        {
            var entity = new PutAwayRule
            {
                Id = 1,
                UnitId = 10,
                ItemGroupId = 2,
                ItemCategoryId = 3,
                ItemId = 5,
                WarehouseId = 4
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.ItemGroupId.Should().Be(2);
            entity.ItemCategoryId.Should().Be(3);
            entity.ItemId.Should().Be(5);
            entity.WarehouseId.Should().Be(4);
        }

        [Fact]
        public void PutAwayRule_NullableItemId_ShouldAcceptNull()
        {
            var entity = new PutAwayRule { ItemId = null };
            entity.ItemId.Should().BeNull();
        }

        [Fact]
        public void PutAwayRule_StrategiesCollection_ShouldDefaultToEmpty()
        {
            var entity = new PutAwayRule();
            entity.Strategies.Should().NotBeNull();
            entity.Strategies.Should().BeEmpty();
        }
    }
}
