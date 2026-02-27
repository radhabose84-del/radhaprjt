using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class SalesItemPriceMasterEntityTests
    {
        [Fact]
        public void SalesItemPriceMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SalesItemPriceMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SalesItemPriceMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesItemPriceMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SalesItemPriceMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesItemPriceMaster)).Should().BeTrue();
        }

        [Fact]
        public void SalesItemPriceMaster_Properties_ShouldBeAssignable()
        {
            var validFrom = DateOnly.FromDateTime(DateTime.UtcNow);
            var validTo = validFrom.AddMonths(6);

            var entity = new SalesItemPriceMaster
            {
                Id = 5,
                PriceCode = "PRC001",
                ItemId = 100,
                SalesSegmentId = 4,
                PaymentTermsId = 2,
                ExMillPrice = 1500.50m,
                CurrencyId = 1,
                ValidFrom = validFrom,
                ValidTo = validTo
            };

            entity.Id.Should().Be(5);
            entity.PriceCode.Should().Be("PRC001");
            entity.ItemId.Should().Be(100);
            entity.SalesSegmentId.Should().Be(4);
            entity.PaymentTermsId.Should().Be(2);
            entity.ExMillPrice.Should().Be(1500.50m);
            entity.CurrencyId.Should().Be(1);
            entity.ValidFrom.Should().Be(validFrom);
            entity.ValidTo.Should().Be(validTo);
        }

        [Fact]
        public void SalesItemPriceMaster_Navigation_ShouldBeAssignable()
        {
            var entity = new SalesItemPriceMaster
            {
                SalesSegment = new SalesSegment()
            };

            entity.SalesSegment.Should().NotBeNull();
        }
    }
}
