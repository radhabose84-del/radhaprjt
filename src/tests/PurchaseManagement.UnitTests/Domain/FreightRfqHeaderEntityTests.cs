using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.FreightRfq;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class FreightRfqHeaderEntityTests
    {
        [Fact]
        public void FreightRfqHeader_DefaultIsActive_ShouldBeActive()
        {
            new FreightRfqHeader().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FreightRfqHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new FreightRfqHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FreightRfqHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FreightRfqHeader)).Should().BeTrue();
        }

        [Fact]
        public void FreightRfqHeader_Quotations_ShouldDefaultToEmptyCollection()
        {
            new FreightRfqHeader().Quotations.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void FreightRfqHeader_Properties_ShouldBeAssignable()
        {
            var entity = new FreightRfqHeader
            {
                Id = 1,
                FreightRfqNumber = "FRFQ-2026-0005",
                RfqTypeId = 1,
                PoReferenceId = 10,
                SupplierId = 5,
                SourceStation = "Adilabad Yard",
                DestinationStation = "Dindigul Mill Gate",
                TotalQuantity = 120.5m,
                TotalBaleCount = 700,
                StatusId = 2
            };

            entity.Id.Should().Be(1);
            entity.FreightRfqNumber.Should().Be("FRFQ-2026-0005");
            entity.PoReferenceId.Should().Be(10);
            entity.TotalQuantity.Should().Be(120.5m);
            entity.TotalBaleCount.Should().Be(700);
        }

        [Fact]
        public void FreightRfqHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new FreightRfqHeader
            {
                PoReferenceId = null,
                SupplierId = null,
                SelectedQuotationId = null,
                ComparisonRemarks = null,
                ApprovedTransporterId = null,
                ApprovedRate = null,
                ApprovedFreightValue = null,
                ApprovalRemarks = null
            };

            entity.PoReferenceId.Should().BeNull();
            entity.SelectedQuotationId.Should().BeNull();
            entity.ApprovedFreightValue.Should().BeNull();
        }
    }
}
