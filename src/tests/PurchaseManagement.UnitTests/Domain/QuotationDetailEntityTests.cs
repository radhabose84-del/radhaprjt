using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class QuotationDetailEntityTests
    {
        [Fact]
        public void QuotationDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QuotationDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void QuotationDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QuotationDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void QuotationDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QuotationDetail)).Should().BeTrue();
        }

        [Fact]
        public void QuotationDetail_Properties_ShouldBeAssignable()
        {
            var entity = new QuotationDetail
            {
                Id = 1,
                QuotationHeaderId = 5,
                ItemId = 10,
                HsnId = 15,
                Quantity = 100m,
                UomId = 20,
                CurrencyId = 25,
                Rate = 50m,
                Discount = 5m,
                GstPercent = 18m,
                LineSubtotal = 5000m,
                GstAmount = 900m,
                Total = 5900m
            };

            entity.Id.Should().Be(1);
            entity.QuotationHeaderId.Should().Be(5);
            entity.ItemId.Should().Be(10);
            entity.Quantity.Should().Be(100m);
            entity.Rate.Should().Be(50m);
            entity.GstPercent.Should().Be(18m);
            entity.Total.Should().Be(5900m);
        }

        [Fact]
        public void QuotationDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new QuotationDetail
            {
                Discount = null,
                Warranty = null,
                ValidityDays = null,
                DeliveryDays = null,
                DiscountTypeId = null,
                PandFCharge = null,
                MiscQuoDiscountType = null
            };

            entity.Discount.Should().BeNull();
            entity.Warranty.Should().BeNull();
            entity.DiscountTypeId.Should().BeNull();
            entity.MiscQuoDiscountType.Should().BeNull();
        }

        [Fact]
        public void QuotationDetail_ConfirmedLinesDetails_DefaultsToEmptyList()
        {
            var entity = new QuotationDetail();

            entity.ConfirmedLinesDetails.Should().NotBeNull();
            entity.ConfirmedLinesDetails.Should().BeEmpty();
        }
    }
}
