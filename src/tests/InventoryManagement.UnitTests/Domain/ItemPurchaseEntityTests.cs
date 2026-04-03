using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemPurchaseEntityTests
    {
        [Fact]
        public void ItemPurchase_Properties_ShouldBeAssignable()
        {
            var entity = new ItemPurchase
            {
                Id = 1,
                ItemId = 10,
                PurchaseUomId = 3,
                LeadTimeDays = 7,
                GrProcessingTimeDays = 2,
                AutomaticPo = true,
                SourceOfItem = 5
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.PurchaseUomId.Should().Be(3);
            entity.LeadTimeDays.Should().Be(7);
            entity.GrProcessingTimeDays.Should().Be(2);
            entity.AutomaticPo.Should().BeTrue();
            entity.SourceOfItem.Should().Be(5);
        }

        [Fact]
        public void ItemPurchase_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemPurchase
            {
                PurchaseUomId = null,
                LeadTimeDays = null,
                GrProcessingTimeDays = null,
                SourceOfItem = null
            };

            entity.PurchaseUomId.Should().BeNull();
            entity.LeadTimeDays.Should().BeNull();
            entity.GrProcessingTimeDays.Should().BeNull();
            entity.SourceOfItem.Should().BeNull();
        }

        [Fact]
        public void ItemPurchase_DefaultAutomaticPo_ShouldBeFalse()
        {
            var entity = new ItemPurchase();
            entity.AutomaticPo.Should().BeFalse();
        }
    }
}
