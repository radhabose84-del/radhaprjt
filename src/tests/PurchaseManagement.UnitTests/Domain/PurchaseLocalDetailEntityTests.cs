using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PurchaseLocalDetailEntityTests
    {
        [Fact]
        public void PurchaseLocalDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PurchaseLocalDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PurchaseLocalDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PurchaseLocalDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PurchaseLocalDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PurchaseLocalDetail)).Should().BeTrue();
        }

        [Fact]
        public void PurchaseLocalDetail_Properties_ShouldBeAssignable()
        {
            var entity = new PurchaseLocalDetail
            {
                Id = 1,
                PurchaseLocalId = 5,
                IndentId = 7,
                ItemSno = 1,
                ItemId = 100,
                UOMId = 10,
                Quantity = 50m,
                UnitPrice = 200m,
                LastPOPrice = 195m,
                DiscountValue = 10m,
                PandFCharge = 50m,
                OtherCharge = 25m,
                GSTPercentage = 18m,
                CGSTPercentage = 9m,
                SGSTPercentage = 9m,
                CGST = 900m,
                SGST = 900m,
                ItemValue = 10000m
            };

            entity.Id.Should().Be(1);
            entity.PurchaseLocalId.Should().Be(5);
            entity.ItemId.Should().Be(100);
            entity.Quantity.Should().Be(50m);
            entity.UnitPrice.Should().Be(200m);
            entity.GSTPercentage.Should().Be(18m);
            entity.ItemValue.Should().Be(10000m);
        }

        [Fact]
        public void PurchaseLocalDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PurchaseLocalDetail
            {
                IndentId = null,
                LastPOPrice = null,
                DiscountTypeId = null,
                MiscDiscountType = null,
                DiscountValue = null,
                PandFType = null,
                PandFCharge = null,
                OtherCharge = null,
                GSTPercentage = null,
                CGST = null,
                SGST = null,
                IGST = null,
                ScheduleDate = null,
                DepartmentId = null,
                PurchaseLocal = null
            };

            entity.IndentId.Should().BeNull();
            entity.LastPOPrice.Should().BeNull();
            entity.DiscountTypeId.Should().BeNull();
            entity.PurchaseLocal.Should().BeNull();
        }
    }
}
