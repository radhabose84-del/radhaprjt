using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PriceMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class PriceMasterHeaderEntityTests
    {
        [Fact]
        public void PriceMasterHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PriceMasterHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PriceMasterHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PriceMasterHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PriceMasterHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PriceMasterHeader)).Should().BeTrue();
        }

        [Fact]
        public void PriceMasterHeader_Properties_ShouldBeAssignable()
        {
            var entity = new PriceMasterHeader
            {
                Id = 1,
                ItemId = 10,
                VendorId = 20,
                UomId = 5,
                ValidFrom = new DateOnly(2025, 1, 1),
                ValidTo = new DateOnly(2025, 12, 31),
                StatusId = 1,
                SourceFromId = 2,
                UnitId = 1
            };

            entity.Id.Should().Be(1);
            entity.ItemId.Should().Be(10);
            entity.VendorId.Should().Be(20);
            entity.UomId.Should().Be(5);
            entity.ValidFrom.Should().Be(new DateOnly(2025, 1, 1));
            entity.ValidTo.Should().Be(new DateOnly(2025, 12, 31));
        }

        [Fact]
        public void PriceMasterHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PriceMasterHeader
            {
                ValidTo = null,
                SourceDetailId = null
            };

            entity.ValidTo.Should().BeNull();
            entity.SourceDetailId.Should().BeNull();
        }

        [Fact]
        public void PriceMasterHeader_Details_DefaultsToEmptyList()
        {
            var entity = new PriceMasterHeader();
            entity.Details.Should().NotBeNull();
        }
    }
}
