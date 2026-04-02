using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Budget;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class BudgetDetailEntityTests
    {
        [Fact]
        public void BudgetDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BudgetDetail();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BudgetDetail_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BudgetDetail();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BudgetDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BudgetDetail)).Should().BeTrue();
        }

        [Fact]
        public void BudgetDetail_Properties_ShouldBeAssignable()
        {
            var entity = new BudgetDetail
            {
                Id = 1,
                BudgetId = 10,
                Month = 3,
                BudgetAmount = 5000m
            };

            entity.Id.Should().Be(1);
            entity.BudgetId.Should().Be(10);
            entity.Month.Should().Be(3);
            entity.BudgetAmount.Should().Be(5000m);
        }

        [Fact]
        public void BudgetDetail_BudgetLogCollection_DefaultsToNull()
        {
            var entity = new BudgetDetail();
            entity.BudgetLog.Should().BeNull();
        }
    }
}
