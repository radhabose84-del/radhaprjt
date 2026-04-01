using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.PutAway;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class PutawayStrategyEntityTests
    {
        [Fact]
        public void PutAwayStrategy_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PutAwayStrategy();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PutAwayStrategy_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PutAwayStrategy();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PutAwayStrategy_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PutAwayStrategy)).Should().BeTrue();
        }

        [Fact]
        public void PutAwayStrategy_Properties_ShouldBeAssignable()
        {
            var entity = new PutAwayStrategy
            {
                Id = 1,
                PutAwayRuleId = 2,
                StorageTypeId = 3,
                TargetId = 4,
                PriorityId = 5
            };

            entity.Id.Should().Be(1);
            entity.PutAwayRuleId.Should().Be(2);
            entity.StorageTypeId.Should().Be(3);
            entity.TargetId.Should().Be(4);
            entity.PriorityId.Should().Be(5);
        }

        [Fact]
        public void PutAwayStrategy_NullableTargetId_ShouldAcceptNull()
        {
            var entity = new PutAwayStrategy { TargetId = null };
            entity.TargetId.Should().BeNull();
        }
    }
}
