using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PriceMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PriceMasterDetailEntityTests
    {
        [Fact]
        public void PriceMasterDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PriceMasterDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PriceMasterDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PriceMasterDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PriceMasterDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PriceMasterDetail)).Should().BeTrue();
        }

        [Fact]
        public void PriceMasterDetail_Properties_ShouldBeAssignable()
        {
            var entity = new PriceMasterDetail
            {
                Id = 1,
                PriceMasterHeaderId = 5,
                ScaleQtyFrom = 1m,
                ScaleQtyTo = 100m,
                UnitPrice = 50m,
                CurrencyId = 1
            };

            entity.Id.Should().Be(1);
            entity.PriceMasterHeaderId.Should().Be(5);
            entity.ScaleQtyFrom.Should().Be(1m);
            entity.ScaleQtyTo.Should().Be(100m);
            entity.UnitPrice.Should().Be(50m);
            entity.CurrencyId.Should().Be(1);
        }

        [Fact]
        public void PriceMasterDetail_ScaleQtyTo_ShouldAcceptNull()
        {
            var entity = new PriceMasterDetail
            {
                ScaleQtyTo = null
            };

            entity.ScaleQtyTo.Should().BeNull();
        }
    }
}
