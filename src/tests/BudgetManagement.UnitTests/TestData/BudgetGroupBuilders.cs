using BudgetManagement.Application.BudgetGroups;
using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.BudgetGroups.Commands.UpdateBudgetGroup;
using BudgetManagement.Application.BudgetGroup.Command.DeleteBudgetGroup;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.TestData
{
    public static class BudgetGroupBuilders
    {
        public static CreateBudgetGroupCommand ValidCreateCommand(
            string name = "Test Budget Group",
            string? description = "Test Description",
            int unitId = 1,
            int departmentId = 1,
            int costCenterId = 1,
            int currencyId = 1,
            int budgetTypeId = 10,
            decimal? allocatedPercentage = 50m,
            decimal? allocatedSpindleCost = null,
            bool carryForward = false,
            bool isParent = true) =>
            new CreateBudgetGroupCommand
            {
                Name = name,
                Description = description,
                UnitId = unitId,
                DepartmentId = departmentId,
                CostCenterId = costCenterId,
                CurrencyId = currencyId,
                BudgetTypeId = budgetTypeId,
                AllocatedPercentage = allocatedPercentage,
                AllocatedSpindleCost = allocatedSpindleCost,
                CarryForward = carryForward,
                IsParent = isParent
            };

        public static UpdateBudgetGroupCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated Budget Group",
            string? description = "Updated Description",
            int unitId = 1,
            int departmentId = 1,
            int costCenterId = 1,
            int currencyId = 1,
            int budgetTypeId = 10,
            decimal? allocatedPercentage = 50m,
            decimal? allocatedSpindleCost = null,
            bool carryForward = false,
            bool isParent = true,
            bool isActive = true) =>
            new UpdateBudgetGroupCommand
            {
                Id = id,
                Name = name,
                Description = description,
                UnitId = unitId,
                DepartmentId = departmentId,
                CostCenterId = costCenterId,
                CurrencyId = currencyId,
                BudgetTypeId = budgetTypeId,
                AllocatedPercentage = allocatedPercentage,
                AllocatedSpindleCost = allocatedSpindleCost,
                CarryForward = carryForward,
                IsParent = isParent,
                IsActive = isActive
            };

        public static DeleteBudgetGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeleteBudgetGroupCommand { Id = id };

        public static BudgetGroupDto ValidDto(
            int id = 1,
            string name = "Test Budget Group",
            int unitId = 1,
            int departmentId = 1,
            int costCenterId = 1) =>
            new BudgetGroupDto
            {
                Id = id,
                Name = name,
                UnitId = unitId,
                DepartmentId = departmentId,
                CostCenterId = costCenterId,
                CurrencyId = 1,
                IsActive = true
            };

        public static BudgetGroupListItemDto ValidListItemDto(
            int id = 1,
            string name = "Test Budget Group") =>
            new BudgetGroupListItemDto
            {
                Id = id,
                Name = name,
                UnitId = 1,
                UnitName = "Unit 1",
                DepartmentId = 1,
                DepartmentName = "Dept 1",
                CostCenterId = 1,
                CostCenterName = "CC 1",
                CurrencyId = 1,
                IsActive = true
            };

        public static BudgetGroupAutoCompleteDto ValidAutoCompleteDto(
            int id = 1,
            string name = "Test Budget Group") =>
            new BudgetGroupAutoCompleteDto
            {
                Id = id,
                Name = name
            };

        public static BudgetManagement.Domain.Entities.BudgetGroup ValidEntity(int id = 1) =>
            new BudgetManagement.Domain.Entities.BudgetGroup
            {
                Id = id,
                Name = "Test Budget Group",
                Description = "Test Description",
                UnitId = 1,
                DepartmentId = 1,
                CostCenterId = 1,
                CurrencyId = 1,
                BudgetTypeId = 10,
                CarryForward = false,
                IsParent = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
