using InventoryManagement.Domain.Entities.MRS;

namespace InventoryManagement.UnitTests.Domain
{
    public class MrsDetailEntityTests
    {
        [Fact]
        public void MrsDetail_Properties_ShouldBeAssignable()
        {
            var entity = new MrsDetail
            {
                Id = 1,
                MrsHeaderId = 10,
                ItemId = 20,
                UomId = 3,
                RequestQuantity = 50m,
                CostCenterId = 4,
                FinanceCode = 5,
                WarehouseStockId = 6
            };

            entity.Id.Should().Be(1);
            entity.MrsHeaderId.Should().Be(10);
            entity.ItemId.Should().Be(20);
            entity.UomId.Should().Be(3);
            entity.RequestQuantity.Should().Be(50m);
            entity.CostCenterId.Should().Be(4);
            entity.FinanceCode.Should().Be(5);
            entity.WarehouseStockId.Should().Be(6);
        }

        [Fact]
        public void MrsDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MrsDetail
            {
                CostCenterId = null,
                FinanceCode = null
            };

            entity.CostCenterId.Should().BeNull();
            entity.FinanceCode.Should().BeNull();
        }

        [Fact]
        public void MrsDetail_DefaultRequestQuantity_ShouldBeZero()
        {
            var entity = new MrsDetail();
            entity.RequestQuantity.Should().Be(0m);
        }
    }
}
