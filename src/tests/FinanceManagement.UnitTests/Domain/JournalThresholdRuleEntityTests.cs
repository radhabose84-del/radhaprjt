using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.UnitTests.Domain
{
    public class JournalThresholdRuleEntityTests
    {
        [Fact]
        public void DefaultIsActive_ShouldBeActive() => new JournalThresholdRule().IsActive.Should().Be(Status.Active);

        [Fact]
        public void DefaultIsDeleted_ShouldBeNotDeleted() => new JournalThresholdRule().IsDeleted.Should().Be(IsDelete.NotDeleted);

        [Fact]
        public void ShouldInheritFromBaseEntity() =>
            typeof(BaseEntity).IsAssignableFrom(typeof(JournalThresholdRule)).Should().BeTrue();

        [Fact]
        public void Properties_ShouldBeAssignable()
        {
            var entity = new JournalThresholdRule
            {
                RuleTypeId = 131,
                ThresholdValue = 5000000m,
                Active = true,
                EffectiveFrom = new DateOnly(2026, 4, 1)
            };

            entity.RuleTypeId.Should().Be(131);
            entity.ThresholdValue.Should().Be(5000000m);
            entity.Active.Should().BeTrue();
            entity.EffectiveFrom.Should().Be(new DateOnly(2026, 4, 1));
        }
    }
}
