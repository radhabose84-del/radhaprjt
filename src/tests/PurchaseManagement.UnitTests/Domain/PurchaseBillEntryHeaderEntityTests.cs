using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseBillEntryHeaderEntityTests
    {
        [Fact]
        public void PurchaseBillEntryHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseBillEntryHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseBillEntryHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseBillEntryHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseBillEntryHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseBillEntryHeader)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseBillEntryHeader_Properties_ShouldBeAssignable()
        {
            var billDate = new DateOnly(2026, 1, 15);
            var entity = new PurchaseBillEntryHeader
            {
                Id = 1,
                UnitId = 10,
                BillNumber = "BILL001",
                BillDate = billDate,
                PartyId = 100,
                PoId = 50,
                GrnId = 25,
                POCategoryId = 5,
                POMethodId = 7,
                SubTotal = 10000m,
                DiscountTotal = 500m,
                TaxableAmount = 9500m,
                CgstAmount = 855m,
                SgstAmount = 855m,
                IgstAmount = 0m,
                OtherCharges = 100m,
                RoundOff = 0.10m,
                GrandTotal = 11310m,
                AttachmentPath = "/path/bill.pdf",
                Remarks = "Test bill",
                IsBillAccounted = true
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.BillNumber.Should().Be("BILL001");
            entity.BillDate.Should().Be(billDate);
            entity.PartyId.Should().Be(100);
            entity.SubTotal.Should().Be(10000m);
            entity.GrandTotal.Should().Be(11310m);
            entity.IsBillAccounted.Should().BeTrue();
        }

        [Fact]
        public void PurchaseBillEntryHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseBillEntryHeader
            {
                PoId = null,
                GrnId = null,
                AttachmentPath = null,
                Remarks = null
            };

            entity.PoId.Should().BeNull();
            entity.GrnId.Should().BeNull();
            entity.AttachmentPath.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void PurchaseBillEntryHeader_Lines_ShouldDefaultToEmptyList()
        {
            var entity = new PurchaseBillEntryHeader();

            entity.Lines.Should().NotBeNull();
            entity.Lines.Should().BeEmpty();
        }
    }
}
