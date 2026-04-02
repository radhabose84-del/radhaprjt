using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Domain
{
    public class ItemSupplierEntityTests
    {
        [Fact]
        public void ItemSupplier_Properties_ShouldBeAssignable()
        {
            var entity = new ItemSupplier
            {
                Id = 1,
                ItemId = 10,
                SupplierId = 20,
                UnitId = 1,
                SupplierPartNo = "SP-001",
                LeadTime = 14,
                MOQ = 100,
                MOQUomId = 3,
                PackageValue = 50m,
                PackageUomId = 4,
                DefaultSupplier = true
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.SupplierId.Should().Be(20);
            entity.UnitId.Should().Be(1);
            entity.SupplierPartNo.Should().Be("SP-001");
            entity.LeadTime.Should().Be(14);
            entity.MOQ.Should().Be(100);
            entity.MOQUomId.Should().Be(3);
            entity.PackageValue.Should().Be(50m);
            entity.PackageUomId.Should().Be(4);
            entity.DefaultSupplier.Should().BeTrue();
        }

        [Fact]
        public void ItemSupplier_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemSupplier
            {
                SupplierPartNo = null,
                LeadTime = null,
                MOQ = null,
                MOQUomId = null,
                PackageValue = null,
                PackageUomId = null,
                DefaultSupplier = null
            };

            entity.SupplierPartNo.Should().BeNull();
            entity.LeadTime.Should().BeNull();
            entity.MOQ.Should().BeNull();
            entity.PackageValue.Should().BeNull();
            entity.DefaultSupplier.Should().BeNull();
        }
    }
}
