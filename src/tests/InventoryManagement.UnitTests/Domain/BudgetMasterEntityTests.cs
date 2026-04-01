using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Budget;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Domain
{
    public class BudgetMasterEntityTests
    {
        [Fact]
        public void BudgetMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BudgetMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BudgetMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BudgetMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BudgetMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BudgetMaster)).Should().BeTrue();
        }

        [Fact]
        public void BudgetMaster_Properties_ShouldBeAssignable()
        {
            var entity = new BudgetMaster
            {
                Id = 1,
                UnitId = 10,
                BudgetGroupId = 5,
                FiscalYear = 2025,
                YearBudgetAmount = 100000m,
                Is_MRApplicable = 1,
                Is_POApplicable = 0,
                Is_ServiceApplicable = 1
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(10);
            entity.BudgetGroupId.Should().Be(5);
            entity.FiscalYear.Should().Be(2025);
            entity.YearBudgetAmount.Should().Be(100000m);
            entity.Is_MRApplicable.Should().Be(1);
        }

        [Fact]
        public void BudgetMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BudgetMaster
            {
                Is_MRApplicable = null,
                Is_POApplicable = null,
                Is_ServiceApplicable = null
            };

            entity.Is_MRApplicable.Should().BeNull();
            entity.Is_POApplicable.Should().BeNull();
            entity.Is_ServiceApplicable.Should().BeNull();
        }

        [Fact]
        public void BudgetMaster_BudgetDetailCollection_DefaultsToNull()
        {
            var entity = new BudgetMaster();
            entity.BudgetDetail.Should().BeNull();
        }
    }
}
