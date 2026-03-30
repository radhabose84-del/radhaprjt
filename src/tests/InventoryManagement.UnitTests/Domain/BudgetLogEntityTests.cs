using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Budget;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class BudgetLogEntityTests
    {
        [Fact]
        public void BudgetLog_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BudgetLog();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BudgetLog_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BudgetLog();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BudgetLog_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BudgetLog)).Should().BeTrue();
        }

        [Fact]
        public void BudgetLog_Properties_ShouldBeAssignable()
        {
            var entity = new BudgetLog
            {
                Id = 1,
                BudgetDetailId = 5,
                ActionTypeId = 2,
                OldBudgetAmount = 1000m,
                NewBudgetAmount = 2000m,
                Remarks = "Revised budget"
            };

            entity.Id.Should().Be(1);
            entity.BudgetDetailId.Should().Be(5);
            entity.ActionTypeId.Should().Be(2);
            entity.OldBudgetAmount.Should().Be(1000m);
            entity.NewBudgetAmount.Should().Be(2000m);
            entity.Remarks.Should().Be("Revised budget");
        }

        [Fact]
        public void BudgetLog_NullableRemarks_ShouldAcceptNull()
        {
            var entity = new BudgetLog { Remarks = null };
            entity.Remarks.Should().BeNull();
        }
    }
}
