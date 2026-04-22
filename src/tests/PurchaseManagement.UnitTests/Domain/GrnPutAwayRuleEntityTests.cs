using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Domain
{
    public class GrnPutAwayRuleEntityTests
    {
        [Fact]
        public void GrnPutAwayRule_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GrnPutAwayRule)).Should().BeFalse();
        }

        [Fact]
        public void GrnPutAwayRule_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new GrnPutAwayRule
            {
                Id = 1,
                PutAwayDate = now,
                GrnDetailId = 10,
                UnitId = 5,
                QcAcceptedQtyPurchaseUom = 100m,
                GrnId = 20,
                PoId = 30,
                PoSlNoLocal = 1,
                ItemId = 50,
                WarehouseId = 3,
                StorageTypeId = 2,
                TargetId = 4,
                PriorityId = 1,
                PurchaseUomId = 6,
                StockUomId = 7,
                ConversionFactor = 1.5m,
                QcAcceptedQtyStockUom = 150m,
                Override = true,
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1"
            };

            entity.Id.Should().Be(1);
            entity.GrnDetailId.Should().Be(10);
            entity.QcAcceptedQtyPurchaseUom.Should().Be(100m);
            entity.ConversionFactor.Should().Be(1.5m);
            entity.QcAcceptedQtyStockUom.Should().Be(150m);
            entity.Override.Should().BeTrue();
            entity.CreatedBy.Should().Be(1);
        }

        [Fact]
        public void GrnPutAwayRule_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GrnPutAwayRule
            {
                PutAwayDate = null,
                ConversionFactor = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null
            };

            entity.PutAwayDate.Should().BeNull();
            entity.ConversionFactor.Should().BeNull();
            entity.CreatedDate.Should().BeNull();
        }

        [Fact]
        public void GrnPutAwayRule_NavigationProperty_ShouldBeAssignable()
        {
            var detail = new GrnDetail();
            var entity = new GrnPutAwayRule
            {
                GrnHeaderPutAwayDetailsMaster = detail
            };

            entity.GrnHeaderPutAwayDetailsMaster.Should().BeSameAs(detail);
        }
    }
}
