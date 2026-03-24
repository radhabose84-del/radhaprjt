using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Domain
{
    public class BudgetGroupEntityTests
    {
        [Fact]
        public void BudgetGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BudgetGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BudgetGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BudgetGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BudgetGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BudgetGroup)).Should().BeTrue();
        }

        [Fact]
        public void BudgetGroup_Properties_ShouldBeAssignable()
        {
            var entity = new BudgetGroup
            {
                Id = 1,
                Name = "Test Group",
                Description = "Desc",
                UnitId = 2,
                DepartmentId = 3,
                CostCenterId = 4,
                CurrencyId = 5,
                CarryForward = true,
                IsParent = true,
                AllocatedPercentage = 50m,
                AllocatedSpindleCost = 100m
            };

            entity.Id.Should().Be(1);
            entity.Name.Should().Be("Test Group");
            entity.UnitId.Should().Be(2);
            entity.DepartmentId.Should().Be(3);
            entity.CostCenterId.Should().Be(4);
            entity.CurrencyId.Should().Be(5);
            entity.CarryForward.Should().BeTrue();
            entity.IsParent.Should().BeTrue();
            entity.AllocatedPercentage.Should().Be(50m);
            entity.AllocatedSpindleCost.Should().Be(100m);
        }

        [Fact]
        public void BudgetGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BudgetGroup
            {
                Name = null,
                Description = null,
                ParentBudgetGroupId = null,
                AllocationRuleId = null,
                AllocatedPercentage = null,
                AllocatedSpindleCost = null,
                BudgetTypeId = null,
                ParentBudgetGroup = null
            };

            entity.Name.Should().BeNull();
            entity.ParentBudgetGroupId.Should().BeNull();
            entity.AllocationRuleId.Should().BeNull();
            entity.AllocatedPercentage.Should().BeNull();
        }

        [Fact]
        public void BudgetGroup_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new BudgetGroup
            {
                ParentBudgetGroup = new BudgetGroup { Id = 99 },
                AllocationRule = new MiscMaster { Id = 10 },
                BudgetType = new MiscMaster { Id = 11 },
                BudgetAllocationGroupType = new List<BudgetAllocation>(),
                BudgetRequestGroupType = new List<BudgetRequest>()
            };

            entity.ParentBudgetGroup!.Id.Should().Be(99);
            entity.AllocationRule!.Id.Should().Be(10);
            entity.BudgetType!.Id.Should().Be(11);
        }
    }
}
