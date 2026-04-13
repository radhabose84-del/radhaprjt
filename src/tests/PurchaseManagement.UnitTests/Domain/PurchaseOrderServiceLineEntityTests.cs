using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseOrderServiceLineEntityTests
    {
        [Fact]
        public void PurchaseOrderServiceLine_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseOrderServiceLine();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseOrderServiceLine_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseOrderServiceLine();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseOrderServiceLine_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseOrderServiceLine)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseOrderServiceLine_Properties_ShouldBeAssignable()
        {
            var entity = new PurchaseOrderServiceLine
            {
                Id = 1,
                PurchaseOrderId = 5,
                ServicePoHeaderId = 7,
                LineNo = 1,
                RequestId = 10,
                ServiceId = 20,
                ServiceCode = "SVC001",
                ServiceDescription = "Test Service",
                UOMId = 3,
                PlannedQuantity = 10m,
                PlannedRate = 500m,
                PlannedValue = 5000m,
                DiscountId = 2,
                Discount = 100m,
                ItemCost = 4900m,
                OtherCost = 50m,
                OtherCharges = 25m,
                GstPercent = 18m,
                Remarks = "Test remarks"
            };

            entity.Id.Should().Be(1);
            entity.PurchaseOrderId.Should().Be(5);
            entity.LineNo.Should().Be(1);
            entity.ServiceCode.Should().Be("SVC001");
            entity.PlannedQuantity.Should().Be(10m);
            entity.PlannedValue.Should().Be(5000m);
        }

        [Fact]
        public void PurchaseOrderServiceLine_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseOrderServiceLine
            {
                RequestId = null,
                ServiceId = null,
                ServiceCode = null,
                ServiceDescription = null,
                UOMId = null,
                DiscountId = null,
                Discount = null,
                ItemCost = null,
                OtherCost = null,
                OtherCharges = null,
                GstPercent = null,
                Remarks = null,
                ServicePoHeader = null
            };

            entity.RequestId.Should().BeNull();
            entity.ServiceCode.Should().BeNull();
            entity.Discount.Should().BeNull();
            entity.ServicePoHeader.Should().BeNull();
        }

        [Fact]
        public void PurchaseOrderServiceLine_PurchaseOrderServiceSchedules_ShouldDefaultToEmptyList()
        {
            var entity = new PurchaseOrderServiceLine();

            entity.PurchaseOrderServiceSchedules.Should().NotBeNull();
            entity.PurchaseOrderServiceSchedules.Should().BeEmpty();
        }
    }
}
