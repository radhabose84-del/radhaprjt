using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseOrderServiceHeaderEntityTests
    {
        [Fact]
        public void PurchaseOrderServiceHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseOrderServiceHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseOrderServiceHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseOrderServiceHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseOrderServiceHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseOrderServiceHeader)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseOrderServiceHeader_Properties_ShouldBeAssignable()
        {
            var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var entity = new PurchaseOrderServiceHeader
            {
                Id = 1,
                PurchaseOrderId = 5,
                ServiceCategoryId = 3,
                ContractTypeId = 2,
                FrequencyId = 4,
                ValidityFrom = from,
                ValidityTo = to,
                TotalOccurrences = 12,
                OverallLimit = 100000m,
                CostCenterId = 7,
                ModeOfDispatchId = 1,
                FreightCharges = 500m,
                TermsId = 8,
                TermDescription = "Standard terms",
                DeliveryAddress = "123 Test St",
                BillingAddress = "456 Bill St",
                POImage = "/path/po.jpg"
            };

            entity.Id.Should().Be(1);
            entity.PurchaseOrderId.Should().Be(5);
            entity.ServiceCategoryId.Should().Be(3);
            entity.ValidityFrom.Should().Be(from);
            entity.ValidityTo.Should().Be(to);
            entity.TotalOccurrences.Should().Be(12);
            entity.OverallLimit.Should().Be(100000m);
        }

        [Fact]
        public void PurchaseOrderServiceHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseOrderServiceHeader
            {
                ContractTypeId = null,
                FrequencyId = null,
                ValidityFrom = null,
                ValidityTo = null,
                TotalOccurrences = null,
                OverallLimit = null,
                CostCenterId = null,
                ModeOfDispatchId = null,
                FreightCharges = null,
                TermsId = null,
                TermDescription = null,
                DeliveryAddress = null,
                BillingAddress = null,
                POImage = null
            };

            entity.ContractTypeId.Should().BeNull();
            entity.ValidityFrom.Should().BeNull();
            entity.OverallLimit.Should().BeNull();
        }

        [Fact]
        public void PurchaseOrderServiceHeader_Items_ShouldDefaultToEmptyList()
        {
            var entity = new PurchaseOrderServiceHeader();

            entity.Items.Should().NotBeNull();
            entity.Items.Should().BeEmpty();
        }
    }
}
