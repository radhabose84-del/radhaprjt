using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class RawMaterialPODetailEntityTests
    {
        [Fact]
        public void RawMaterialPODetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RawMaterialPODetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RawMaterialPODetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RawMaterialPODetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RawMaterialPODetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RawMaterialPODetail)).Should().BeTrue();
        }

        [Fact]
        public void RawMaterialPODetail_Properties_ShouldBeAssignable()
        {
            var entity = new RawMaterialPODetail
            {
                Id = 1,
                POHeaderId = 2,
                ItemId = 3,
                HsnId = 4,
                Quantity = 500m,
                Weight = 85000m,
                Rate = 68500m,
                ItemValue = 34_250_000m,
                CGSTValue = 856_250m,
                SGSTValue = 856_250m,
                IGSTValue = 0m,
                TotalGST = 1_712_500m,
                NetValue = 35_962_500m
            };

            entity.POHeaderId.Should().Be(2);
            entity.ItemId.Should().Be(3);
            entity.HsnId.Should().Be(4);
            entity.Quantity.Should().Be(500m);
            entity.NetValue.Should().Be(35_962_500m);
        }

        [Fact]
        public void RawMaterialPODetail_NullableWeight_ShouldAcceptNull()
        {
            var entity = new RawMaterialPODetail { Weight = null };
            entity.Weight.Should().BeNull();
        }
    }
}
