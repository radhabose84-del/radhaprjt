using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;

namespace PurchaseManagement.UnitTests.Domain
{
    public class QuotationComparisonDetailEntityTests
    {
        [Fact]
        public void QuotationComparisonDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QuotationComparisonDetail)).Should().BeFalse();
        }

        [Fact]
        public void QuotationComparisonDetail_Properties_ShouldBeAssignable()
        {
            var entity = new QuotationComparisonDetail
            {
                Id = 1,
                QuotationComparisonHeaderId = 10,
                QuotationHeaderId = 20,
                QuotationDetailId = 30,
                Net = 1000m,
                LandedUnit = 55m,
                Total = 5500m,
                OverrideStatus = true,
                Remarks = "Best price"
            };

            entity.Id.Should().Be(1);
            entity.QuotationComparisonHeaderId.Should().Be(10);
            entity.QuotationHeaderId.Should().Be(20);
            entity.QuotationDetailId.Should().Be(30);
            entity.Net.Should().Be(1000m);
            entity.LandedUnit.Should().Be(55m);
            entity.Total.Should().Be(5500m);
            entity.OverrideStatus.Should().BeTrue();
            entity.Remarks.Should().Be("Best price");
        }

        [Fact]
        public void QuotationComparisonDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new QuotationComparisonDetail
            {
                Remarks = null
            };

            entity.Remarks.Should().BeNull();
        }

        [Fact]
        public void QuotationComparisonDetail_NavigationProperties_ShouldBeAssignable()
        {
            var compHeader = new QuotationComparisonHeader();
            var quotHeader = new QuotationHeader();
            var quotDetail = new QuotationDetail();

            var entity = new QuotationComparisonDetail
            {
                QuotationComparisonHeader = compHeader,
                QuotationHeader = quotHeader,
                QuotationCompareDetail = quotDetail
            };

            entity.QuotationComparisonHeader.Should().BeSameAs(compHeader);
            entity.QuotationHeader.Should().BeSameAs(quotHeader);
            entity.QuotationCompareDetail.Should().BeSameAs(quotDetail);
        }
    }
}
