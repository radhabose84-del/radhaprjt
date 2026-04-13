using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseBillEntryDetailEntityTests
    {
        [Fact]
        public void PurchaseBillEntryDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseBillEntryDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseBillEntryDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseBillEntryDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseBillEntryDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseBillEntryDetail)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseBillEntryDetail_Properties_ShouldBeAssignable()
        {
            var entity = new PurchaseBillEntryDetail
            {
                Id = 1,
                BillEntryHeaderId = 5,
                GrnDetailId = 10,
                PoDetailId = 15,
                ItemId = 100,
                PoQty = 100m,
                GrnQty = 95m,
                BilledQty = 95m,
                PoRate = 50m,
                BilledRate = 50m,
                UomId = 20,
                TaxPercentage = 18m,
                LineBaseAmount = 4750m,
                DiscountAmount = 100m,
                TaxableAmount = 4650m,
                CgstAmount = 418.50m,
                SgstAmount = 418.50m,
                IgstAmount = 0m,
                LineTotal = 5487m
            };

            entity.Id.Should().Be(1);
            entity.BillEntryHeaderId.Should().Be(5);
            entity.PoDetailId.Should().Be(15);
            entity.ItemId.Should().Be(100);
            entity.PoQty.Should().Be(100m);
            entity.BilledQty.Should().Be(95m);
            entity.LineTotal.Should().Be(5487m);
        }

        [Fact]
        public void PurchaseBillEntryDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseBillEntryDetail
            {
                GrnDetailId = null,
                UomId = null
            };

            entity.GrnDetailId.Should().BeNull();
            entity.UomId.Should().BeNull();
        }
    }
}
