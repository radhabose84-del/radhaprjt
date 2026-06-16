using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class TaxAccountLinkageEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() =>
            new TaxAccountLinkage().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() =>
            new TaxAccountLinkage().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(FinanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(TaxAccountLinkage)).Should().BeTrue();

        [Fact]
        public void ShouldImplementIActivityTracked() =>
            typeof(FinanceManagement.Domain.Common.IActivityTracked)
                .IsAssignableFrom(typeof(TaxAccountLinkage)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new TaxAccountLinkage
            {
                Id = 1,
                CompanyId = 1,
                TaxCodeId = 14,
                GlAccountId = 412,
                ControlAccountId = 30,
                IsActive = Status.Inactive,
                StatusId = 19,
                EffectiveFrom = new DateOnly(2026, 5, 25),
                EffectiveTo = null
            };

            entity.TaxCodeId.Should().Be(14);
            entity.GlAccountId.Should().Be(412);
            entity.ControlAccountId.Should().Be(30);
            entity.StatusId.Should().Be(19);
            entity.IsActive.Should().Be(Status.Inactive);
        }
    }
}
