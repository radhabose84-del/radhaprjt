using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseOrderHeaderEntityTests
    {
        [Fact]
        public void PurchaseOrderHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseOrderHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseOrderHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseOrderHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseOrderHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseOrderHeader)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseOrderHeader_DefaultRevisionNo_ShouldBeZero()
        {
            var entity = new PurchaseOrderHeader();
            entity.RevisionNo.Should().Be(0);
        }

        [Fact]
        public void PurchaseOrderHeader_Properties_ShouldBeAssignable()
        {
            var poDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var entity = new PurchaseOrderHeader
            {
                Id = 1,
                UnitId = 10,
                PONumber = "PO0001",
                PODate = poDate,
                POCategoryId = 5,
                CurrencyId = 1,
                VendorId = 100,
                ItemTotal = 50000m,
                GSTTotal = 9000m,
                PurchaseValue = 59000m,
                StatusId = 2,
                RevisionNo = 1,
                AmendmentReason = "Quantity revised"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.PONumber.Should().Be("PO0001");
            entity.PODate.Should().Be(poDate);
            entity.VendorId.Should().Be(100);
            entity.ItemTotal.Should().Be(50000m);
            entity.PurchaseValue.Should().Be(59000m);
            entity.RevisionNo.Should().Be(1);
            entity.AmendmentReason.Should().Be("Quantity revised");
        }

        [Fact]
        public void PurchaseOrderHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseOrderHeader
            {
                POMethodId = null,
                DiscountTotal = null,
                FreightTotal = null,
                AdvanceAmount = null,
                OldPOId = null,
                AmendmentReason = null,
                CapitalTypeId = null,
                ProjectId = null
            };

            entity.POMethodId.Should().BeNull();
            entity.DiscountTotal.Should().BeNull();
            entity.OldPOId.Should().BeNull();
            entity.ProjectId.Should().BeNull();
        }

        [Fact]
        public void PurchaseOrderHeader_NavigationCollections_ShouldDefaultToEmptyList()
        {
            var entity = new PurchaseOrderHeader();

            entity.Headers.Should().NotBeNull();
            entity.PaymentTerms.Should().NotBeNull();
            entity.POGateEntriesDetails.Should().NotBeNull();
            entity.PoGrnDetails.Should().NotBeNull();
            entity.ServicePos.Should().NotBeNull();
            entity.ServiceEntrySheets.Should().NotBeNull();
            entity.ImportPOHeader.Should().NotBeNull();
        }
    }
}
