using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Domain
{
    public class GrnDetailEntityTests
    {
        [Fact]
        public void GrnDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GrnDetail)).Should().BeFalse();
        }

        [Fact]
        public void GrnDetail_Properties_ShouldBeAssignable()
        {
            var entity = new GrnDetail
            {
                Id = 1,
                GrnId = 10,
                PoId = 100,
                PoSlNoLocal = 5,
                PoCategoryId = 2,
                PoMethodId = 3,
                ItemId = 50,
                OrderQuantity = 100m,
                DcQuantity = 90m,
                UpperTolerance = 5m,
                LowerTolerance = 2m,
                ReceivedQuantity = 88m,
                BatchNumber = "BATCH001",
                QcAcceptedQuantity = 85m,
                QcRejectedQuantity = 3m,
                QcRejectedRemarks = "Damaged",
                ItemValue = 5000m,
                UnitPrice = 50m,
                DiscountValue = 100m,
                CGST = 9m,
                SGST = 9m,
                IGST = 0m,
                GSTPercentage = 18m,
                UOMId = 1,
                TaxableAmount = 4900m
            };

            entity.Id.Should().Be(1);
            entity.GrnId.Should().Be(10);
            entity.PoId.Should().Be(100);
            entity.PoSlNoLocal.Should().Be(5);
            entity.ItemId.Should().Be(50);
            entity.OrderQuantity.Should().Be(100m);
            entity.ReceivedQuantity.Should().Be(88m);
            entity.QcAcceptedQuantity.Should().Be(85m);
            entity.UnitPrice.Should().Be(50m);
            entity.TaxableAmount.Should().Be(4900m);
        }

        [Fact]
        public void GrnDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GrnDetail
            {
                PoSlNoLocal = null,
                UpperTolerance = null,
                LowerTolerance = null,
                ExpiryDate = null,
                BatchNumber = null,
                QcAcceptedQuantity = null,
                QcRejectedQuantity = null,
                QcRejectedRemarks = null,
                ItemValue = null,
                UnitPrice = null,
                DiscountValue = null,
                CGST = null,
                SGST = null,
                IGST = null,
                GSTPercentage = null,
                UOMId = null,
                TaxableAmount = null,
                PoGrnCategoryDetails = null,
                PoGrnMethodDetails = null,
                GrnPutAwayDetails = null
            };

            entity.PoSlNoLocal.Should().BeNull();
            entity.ExpiryDate.Should().BeNull();
            entity.BatchNumber.Should().BeNull();
            entity.GrnPutAwayDetails.Should().BeNull();
        }

        [Fact]
        public void GrnDetail_NavigationProperties_ShouldBeAssignable()
        {
            var header = new GrnHeader();
            var po = new PurchaseOrderHeader();
            var putAway = new List<GrnPutAwayRule> { new GrnPutAwayRule() };

            var entity = new GrnDetail
            {
                GrnHeaderDetailsMaster = header,
                GrnPoDetails = po,
                GrnPutAwayDetails = putAway
            };

            entity.GrnHeaderDetailsMaster.Should().BeSameAs(header);
            entity.GrnPoDetails.Should().BeSameAs(po);
            entity.GrnPutAwayDetails.Should().HaveCount(1);
        }
    }
}
