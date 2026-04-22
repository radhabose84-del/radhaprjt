using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.UnitTests.Domain
{
    public class MrsDetailEntityTests
    {
        [Fact]
        public void MrsDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MrsDetail)).Should().BeFalse();
        }

        [Fact]
        public void MrsDetail_Properties_ShouldBeAssignable()
        {
            var entity = new MrsDetail
            {
                Id = 1,
                MrsHeaderId = 10,
                ItemId = 50,
                UomId = 3,
                RequestQuantity = 100m,
                CostCenterId = 7,
                FinanceCode = 1001,
                WarehouseStockId = 5
            };

            entity.Id.Should().Be(1);
            entity.MrsHeaderId.Should().Be(10);
            entity.ItemId.Should().Be(50);
            entity.UomId.Should().Be(3);
            entity.RequestQuantity.Should().Be(100m);
            entity.CostCenterId.Should().Be(7);
            entity.FinanceCode.Should().Be(1001);
            entity.WarehouseStockId.Should().Be(5);
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
        public void MrsDetail_NavigationProperty_ShouldBeAssignable()
        {
            var header = new MrsHeader();
            var entity = new MrsDetail
            {
                MrsHeaderDetails = header
            };

            entity.MrsHeaderDetails.Should().BeSameAs(header);
        }
    }
}
