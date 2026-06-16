using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class TaxCodeMasterEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new TaxCodeMaster().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new TaxCodeMaster().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(TaxCodeMaster)).Should().BeTrue();

        [Fact]
        public void ShouldImplementIActivityTracked() =>
            typeof(FinanceManagement.Domain.Common.IActivityTracked)
                .IsAssignableFrom(typeof(TaxCodeMaster)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new TaxCodeMaster
            {
                Id = 1,
                CompanyId = 1,
                TaxCode = "GST-OUT-5",
                TaxName = "GST Output 5%",
                TaxTypeId = 10,
                TaxComponentId = 20,
                ParentTaxCodeId = null,
                DirectionId = 30,
                StatutorySection = null,
                ThresholdAmount = null,
                ThresholdAggregate = null,
                HsnSacCode = "5205",
                IsSystemOnlyPosting = true,
                IsEefcRelevant = false,
                IsStatutoryFixed = true
            };

            entity.TaxCode.Should().Be("GST-OUT-5");
            entity.TaxTypeId.Should().Be(10);
            entity.TaxComponentId.Should().Be(20);
            entity.DirectionId.Should().Be(30);
            entity.IsSystemOnlyPosting.Should().BeTrue();
        }

        [Fact]
        public void ComponentChild_CanReferenceParent()
        {
            var entity = new TaxCodeMaster { TaxComponentId = 21, ParentTaxCodeId = 10 };
            entity.ParentTaxCodeId.Should().Be(10);
        }

        [Fact]
        public void TdsThresholds_AcceptValues()
        {
            var entity = new TaxCodeMaster { TaxTypeId = 13, StatutorySection = "194C", ThresholdAmount = 30000m, ThresholdAggregate = 100000m };
            entity.ThresholdAmount.Should().Be(30000m);
            entity.ThresholdAggregate.Should().Be(100000m);
        }
    }
}
