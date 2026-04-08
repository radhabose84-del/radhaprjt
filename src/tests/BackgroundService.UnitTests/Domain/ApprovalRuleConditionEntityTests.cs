using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Workflow;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class ApprovalRuleConditionEntityTests
    {
        [Fact]
        public void ApprovalRuleCondition_DefaultIsActive_ShouldBeActive()
        {
            var entity = new ApprovalRuleCondition();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ApprovalRuleCondition_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new ApprovalRuleCondition();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ApprovalRuleCondition_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ApprovalRuleCondition)).Should().BeTrue();
        }

        [Fact]
        public void ApprovalRuleCondition_Properties_ShouldBeAssignable()
        {
            var entity = new ApprovalRuleCondition
            {
                Id = 1,
                RuleId = 5,
                FieldId = 3,
                OperatorId = 2,
                RightTypeId = 4,
                RightValue = "100",
                Aggregate = "SUM"
            };

            entity.Id.Should().Be(1);
            entity.RuleId.Should().Be(5);
            entity.FieldId.Should().Be(3);
            entity.OperatorId.Should().Be(2);
            entity.RightTypeId.Should().Be(4);
            entity.RightValue.Should().Be("100");
            entity.Aggregate.Should().Be("SUM");
        }

        [Fact]
        public void ApprovalRuleCondition_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ApprovalRuleCondition
            {
                Aggregate = null
            };

            entity.Aggregate.Should().BeNull();
        }
    }
}
