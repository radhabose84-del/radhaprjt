using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class ItemPriceMasterEntityTests
    {
        [Fact]
        public void ItemPriceMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ItemPriceMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ItemPriceMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ItemPriceMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ItemPriceMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ItemPriceMaster)).Should().BeTrue();
        }

        [Fact]
        public void ItemPriceMaster_Properties_ShouldBeAssignable()
        {
            var validFrom = DateOnly.FromDateTime(DateTime.UtcNow);
            var validTo = validFrom.AddMonths(6);

            var entity = new ItemPriceMaster
            {
                Id = 5,
                PriceCode = "PRC001",
                ItemId = 100,
                SalesSegmentId = 4,
                PaymentTermsId = 2,
                ExMillRate = 1500.50m,
                CurrencyId = 1,
                ValidFrom = validFrom,
                ValidTo = validTo
            };

            entity.Id.Should().Be(5);
            entity.PriceCode.Should().Be("PRC001");
            entity.ItemId.Should().Be(100);
            entity.SalesSegmentId.Should().Be(4);
            entity.PaymentTermsId.Should().Be(2);
            entity.ExMillRate.Should().Be(1500.50m);
            entity.CurrencyId.Should().Be(1);
            entity.ValidFrom.Should().Be(validFrom);
            entity.ValidTo.Should().Be(validTo);
        }

        [Fact]
        public void ItemPriceMaster_Navigation_ShouldBeAssignable()
        {
            var entity = new ItemPriceMaster
            {
                SalesSegment = new SalesSegment()
            };

            entity.SalesSegment.Should().NotBeNull();
        }
    }
}
