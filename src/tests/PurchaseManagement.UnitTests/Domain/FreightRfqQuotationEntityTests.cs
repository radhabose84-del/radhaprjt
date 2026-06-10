using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.FreightRfq;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class FreightRfqQuotationEntityTests
    {
        [Fact]
        public void FreightRfqQuotation_DefaultIsActive_ShouldBeActive()
        {
            new FreightRfqQuotation().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void FreightRfqQuotation_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new FreightRfqQuotation().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void FreightRfqQuotation_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(FreightRfqQuotation)).Should().BeTrue();
        }

        [Fact]
        public void FreightRfqQuotation_DefaultFlags_ShouldBeFalse()
        {
            var entity = new FreightRfqQuotation();
            entity.IsSelected.Should().BeFalse();
            entity.IsOverride.Should().BeFalse();
        }

        [Fact]
        public void FreightRfqQuotation_Properties_ShouldBeAssignable()
        {
            var entity = new FreightRfqQuotation
            {
                Id = 3,
                FreightRfqHeaderId = 1,
                TransporterId = 102,
                RateBasisId = 21,
                QuotedRate = 7999.99m,
                NoOfVehicles = 4,
                FreightValue = 963999m,
                IsSelected = true,
                IsOverride = true,
                Remarks = "Covered trucks"
            };

            entity.FreightRfqHeaderId.Should().Be(1);
            entity.TransporterId.Should().Be(102);
            entity.QuotedRate.Should().Be(7999.99m);
            entity.FreightValue.Should().Be(963999m);
            entity.IsSelected.Should().BeTrue();
            entity.IsOverride.Should().BeTrue();
        }

        [Fact]
        public void FreightRfqQuotation_NullableProperties_ShouldAcceptNull()
        {
            var entity = new FreightRfqQuotation { NoOfVehicles = null, Remarks = null };
            entity.NoOfVehicles.Should().BeNull();
            entity.Remarks.Should().BeNull();
        }
    }
}
