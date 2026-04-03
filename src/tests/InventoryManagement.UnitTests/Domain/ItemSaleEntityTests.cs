using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemSaleEntityTests
    {
        [Fact]
        public void ItemSale_Properties_ShouldBeAssignable()
        {
            var entity = new ItemSale
            {
                Id = 1,
                ItemId = 10,
                UomId = 3,
                MinQuantity = 5m,
                PackageQuantity = 10m,
                DeliveryLeadTime = 7,
                Discount = true,
                CountId = 2,
                ValuationMethodId = 4
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.UomId.Should().Be(3);
            entity.MinQuantity.Should().Be(5m);
            entity.PackageQuantity.Should().Be(10m);
            entity.DeliveryLeadTime.Should().Be(7);
            entity.Discount.Should().BeTrue();
            entity.CountId.Should().Be(2);
            entity.ValuationMethodId.Should().Be(4);
        }

        [Fact]
        public void ItemSale_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemSale
            {
                UomId = null,
                PackageQuantity = null,
                DeliveryLeadTime = null,
                CountId = null,
                ValuationMethodId = null
            };

            entity.UomId.Should().BeNull();
            entity.PackageQuantity.Should().BeNull();
            entity.DeliveryLeadTime.Should().BeNull();
            entity.CountId.Should().BeNull();
            entity.ValuationMethodId.Should().BeNull();
        }

        [Fact]
        public void ItemSale_DefaultDiscount_ShouldBeFalse()
        {
            var entity = new ItemSale();
            entity.Discount.Should().BeFalse();
        }
    }
}
