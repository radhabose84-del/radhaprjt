using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Domain
{
    public class GrnHeaderEntityTests
    {
        [Fact]
        public void GrnHeader_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GrnHeader)).Should().BeFalse();
        }

        [Fact]
        public void GrnHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new GrnHeader
            {
                Id = 1,
                UnitId = 10,
                GrnNo = "GRN001",
                GrnDate = now,
                GateEntryId = 5,
                PartyId = 20,
                InvoiceNo = "INV001",
                InvoiceDate = now,
                DcNo = "DC001",
                DcDate = now,
                ReceivingWarehouseId = 3,
                Remarks = "Test remarks",
                IsGrnGenerated = true,
                GrnReceivedImage = "image.jpg",
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                QcWarehouseId = 2,
                RejectedImage = "rejected.jpg",
                ItemsTotal = 10000m,
                DiscountTotal = 500m,
                TaxableAmount = 9500m,
                CGSTTotal = 855m,
                SGSTTotal = 855m,
                IGSTTotal = 0m,
                MiscCharges = 100m,
                RoundOff = 0.50m,
                PurchaseValue = 11310.50m
            };

            entity.Id.Should().Be(1);
            entity.GrnNo.Should().Be("GRN001");
            entity.PartyId.Should().Be(20);
            entity.IsGrnGenerated.Should().BeTrue();
            entity.QcWarehouseId.Should().Be(2);
            entity.PurchaseValue.Should().Be(11310.50m);
        }

        [Fact]
        public void GrnHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GrnHeader
            {
                GrnNo = null,
                InvoiceNo = null,
                DcNo = null,
                DcDate = null,
                Remarks = null,
                GrnReceivedImage = null,
                CreatedDate = null,
                CreatedByName = null,
                CreatedIP = null,
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null,
                QcWarehouseId = null,
                RejectedImage = null,
                ItemsTotal = null,
                DiscountTotal = null,
                TaxableAmount = null,
                CGSTTotal = null,
                SGSTTotal = null,
                IGSTTotal = null,
                MiscCharges = null,
                RoundOff = null,
                PurchaseValue = null,
                GrnDetails = null
            };

            entity.GrnNo.Should().BeNull();
            entity.DcDate.Should().BeNull();
            entity.QcWarehouseId.Should().BeNull();
            entity.GrnDetails.Should().BeNull();
        }

        [Fact]
        public void GrnHeader_NavigationProperties_ShouldBeAssignable()
        {
            var gateEntry = new GateEntryHeader();
            var details = new List<GrnDetail> { new GrnDetail() };

            var entity = new GrnHeader
            {
                GrnHeaderDetails = gateEntry,
                GrnDetails = details
            };

            entity.GrnHeaderDetails.Should().BeSameAs(gateEntry);
            entity.GrnDetails.Should().HaveCount(1);
        }
    }
}
