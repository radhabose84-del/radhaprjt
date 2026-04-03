using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemUsageTypeMappingEntityTests
    {
        [Fact]
        public void ItemUsageTypeMapping_Properties_ShouldBeAssignable()
        {
            var entity = new ItemUsageTypeMapping
            {
                Id = 1,
                ItemId = 10,
                UsageTypeId = 3,
                UnitId = 2
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.UsageTypeId.Should().Be(3);
            entity.UnitId.Should().Be(2);
        }

        [Fact]
        public void ItemUsageTypeMapping_DefaultValues_ShouldBeZero()
        {
            var entity = new ItemUsageTypeMapping();
            entity.Id.Should().Be(0);
            entity.ItemId.Should().Be(0);
            entity.UsageTypeId.Should().Be(0);
            entity.UnitId.Should().Be(0);
        }
    }
}
