using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class TaxCodeRateVersionEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new TaxCodeRateVersion().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new TaxCodeRateVersion().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(TaxCodeRateVersion)).Should().BeTrue();

        [Fact]
        public void ShouldImplementIActivityTracked() =>
            typeof(FinanceManagement.Domain.Common.IActivityTracked)
                .IsAssignableFrom(typeof(TaxCodeRateVersion)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new TaxCodeRateVersion
            {
                Id = 1,
                TaxCodeId = 5,
                VersionNo = 2,
                RatePercent = 12.0m,
                EffectiveFrom = new DateOnly(2026, 7, 1),
                EffectiveTo = null,
                ChangeReason = "HSN reclassification"
            };

            entity.TaxCodeId.Should().Be(5);
            entity.VersionNo.Should().Be(2);
            entity.RatePercent.Should().Be(12.0m);
            entity.EffectiveTo.Should().BeNull();
        }

        [Fact]
        public void ClosedVersion_HasEffectiveTo()
        {
            var entity = new TaxCodeRateVersion { EffectiveFrom = new DateOnly(2017, 7, 1), EffectiveTo = new DateOnly(2026, 6, 30) };
            entity.EffectiveTo.Should().Be(new DateOnly(2026, 6, 30));
        }
    }
}
