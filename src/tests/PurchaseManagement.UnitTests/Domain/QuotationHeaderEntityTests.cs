using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class QuotationHeaderEntityTests
    {
        [Fact]
        public void QuotationHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new QuotationHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void QuotationHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new QuotationHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void QuotationHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(QuotationHeader)).Should().BeTrue();
        }

        [Fact]
        public void QuotationHeader_Properties_ShouldBeAssignable()
        {
            var entity = new QuotationHeader
            {
                Id = 1,
                UnitId = 2,
                RfqId = 3,
                SupplierId = 4,
                QuotationNumber = "Q001",
                ValidTill = DateOnly.FromDateTime(DateTime.Today),
                Freight = 100m,
                GrandTotal = 5000m
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(2);
            entity.RfqId.Should().Be(3);
            entity.SupplierId.Should().Be(4);
            entity.QuotationNumber.Should().Be("Q001");
            entity.Freight.Should().Be(100m);
            entity.GrandTotal.Should().Be(5000m);
        }

        [Fact]
        public void QuotationHeader_Lines_ShouldBeInitialized()
        {
            var entity = new QuotationHeader();

            entity.Lines.Should().NotBeNull();
            entity.Lines.Should().BeEmpty();
        }

        [Fact]
        public void QuotationHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new QuotationHeader
            {
                FreightModeId = null,
                PaymentTermsId = null,
                IncotermsId = null,
                InsuranceCharge = null,
                QuotationImage = null
            };

            entity.FreightModeId.Should().BeNull();
            entity.PaymentTermsId.Should().BeNull();
            entity.IncotermsId.Should().BeNull();
            entity.InsuranceCharge.Should().BeNull();
            entity.QuotationImage.Should().BeNull();
        }
    }
}
