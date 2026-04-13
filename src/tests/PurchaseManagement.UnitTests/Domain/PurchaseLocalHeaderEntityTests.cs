using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseLocalHeaderEntityTests
    {
        [Fact]
        public void PurchaseLocalHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseLocalHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseLocalHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseLocalHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseLocalHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseLocalHeader)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseLocalHeader_Properties_ShouldBeAssignable()
        {
            var entity = new PurchaseLocalHeader
            {
                Id = 1,
                PurchaseOrderId = 5,
                IsPartialReceiptAllowed = true,
                IncotermsId = 3,
                ModeOfDispatchId = 2,
                FreightCharges = 500m,
                TermsId = 7,
                TermDescription = "Net 30 days",
                DeliveryAddress = "123 Test St",
                BillingAddress = "456 Bill St"
            };

            entity.Id.Should().Be(1);
            entity.PurchaseOrderId.Should().Be(5);
            entity.IsPartialReceiptAllowed.Should().BeTrue();
            entity.IncotermsId.Should().Be(3);
            entity.FreightCharges.Should().Be(500m);
            entity.TermDescription.Should().Be("Net 30 days");
        }

        [Fact]
        public void PurchaseLocalHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseLocalHeader
            {
                IncotermsId = null,
                ModeOfDispatchId = null,
                FreightCharges = null,
                TermsId = null,
                TermDescription = null,
                DeliveryAddress = null,
                BillingAddress = null,
                PurchaseLocal = null
            };

            entity.IncotermsId.Should().BeNull();
            entity.FreightCharges.Should().BeNull();
            entity.PurchaseLocal.Should().BeNull();
        }

        [Fact]
        public void PurchaseLocalHeader_Details_ShouldDefaultToEmptyList()
        {
            var entity = new PurchaseLocalHeader();

            entity.Details.Should().NotBeNull();
            entity.Details.Should().BeEmpty();
        }
    }
}
