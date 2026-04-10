using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalRuleEntityTests
    {
        [Fact]
        public void ApprovalRule_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ApprovalRule();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ApprovalRule_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ApprovalRule();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ApprovalRule_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ApprovalRule)).Should().BeTrue();
        }

        [Fact]
        public void ApprovalRule_Properties_ShouldBeAssignable()
        {
            var fromDate = new DateOnly(2026, 1, 1);
            var toDate = new DateOnly(2026, 12, 31);
            var entity = new ApprovalRule
            {
                Id = 1,
                ApprovalStepDetailId = 5,
                Priority = 10,
                ActionId = 3,
                EffectiveFrom = fromDate,
                EffectiveTo = toDate
            };

            entity.Id.Should().Be(1);
            entity.ApprovalStepDetailId.Should().Be(5);
            entity.Priority.Should().Be(10);
            entity.ActionId.Should().Be(3);
            entity.EffectiveFrom.Should().Be(fromDate);
            entity.EffectiveTo.Should().Be(toDate);
        }
    }
}
