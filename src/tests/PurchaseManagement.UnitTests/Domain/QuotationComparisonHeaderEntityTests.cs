using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;

namespace PurchaseManagement.UnitTests.Domain
{
    public class QuotationComparisonHeaderEntityTests
    {
        [Fact]
        public void QuotationComparisonHeader_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new QuotationComparisonHeader
            {
                Id = 1,
                RfqId = 2,
                RfqCode = "RFQ001",
                ConfirmedDate = now,
                CreatedBy = 1,
                CreatedDate = now,
                StatusId = 3
            };

            entity.Id.Should().Be(1);
            entity.RfqId.Should().Be(2);
            entity.RfqCode.Should().Be("RFQ001");
            entity.ConfirmedDate.Should().Be(now);
            entity.StatusId.Should().Be(3);
        }

        [Fact]
        public void QuotationComparisonHeader_Collection_ShouldBeInitialized()
        {
            var entity = new QuotationComparisonHeader();

            entity.QuotationConfirmedDetails.Should().NotBeNull();
            entity.QuotationConfirmedDetails.Should().BeEmpty();
        }

        [Fact]
        public void QuotationComparisonHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new QuotationComparisonHeader
            {
                RfqCode = null,
                CreatedByName = null,
                CreatedIP = null,
                CreatedDate = null,
                ModifiedBy = null,
                ModifiedDate = null,
                ModifiedByName = null,
                ModifiedIP = null
            };

            entity.RfqCode.Should().BeNull();
            entity.CreatedByName.Should().BeNull();
            entity.ModifiedBy.Should().BeNull();
            entity.ModifiedDate.Should().BeNull();
        }

        [Fact]
        public void QuotationComparisonHeader_DoesNotInheritBaseEntity()
        {
            // QuotationComparisonHeader does NOT extend BaseEntity - it has its own Id and audit fields
            typeof(PurchaseManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(QuotationComparisonHeader))
                .Should().BeFalse();
        }
    }
}
