using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class DeliveryScoreRuleEntityTests
    {
        [Fact]
        public void DeliveryScoreRule_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DeliveryScoreRule();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DeliveryScoreRule_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DeliveryScoreRule();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DeliveryScoreRule_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DeliveryScoreRule)).Should().BeTrue();
        }

        [Fact]
        public void DeliveryScoreRule_Properties_ShouldBeAssignable()
        {
            var entity = new DeliveryScoreRule
            {
                Id = 1,
                RuleCode = "DSR001",
                Description = "On-time delivery",
                DelayDaysFrom = 0,
                DelayDaysTo = 0,
                Score = 100m,
                SortOrder = 1
            };

            entity.Id.Should().Be(1);
            entity.RuleCode.Should().Be("DSR001");
            entity.Description.Should().Be("On-time delivery");
            entity.DelayDaysFrom.Should().Be(0);
            entity.DelayDaysTo.Should().Be(0);
            entity.Score.Should().Be(100m);
            entity.SortOrder.Should().Be(1);
        }

        [Fact]
        public void DeliveryScoreRule_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DeliveryScoreRule
            {
                RuleCode = null,
                Description = null
            };

            entity.RuleCode.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
