using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.BudgetAllocation.Command.Update;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.TestData
{
    public static class BudgetAllocationBuilders
    {
        public static CreateBudgetAllocationDto ValidCreateDto(
            int financialYearId = 1,
            int requestById = 1,
            int unitId = 1,
            int budgetGroupId = 1,
            decimal approvedAmount = 50000m) =>
            new CreateBudgetAllocationDto
            {
                FinancialYearId = financialYearId,
                RequestById = requestById,
                RequestMonthId = 1,
                UnitId = unitId,
                BudgetGroupId = budgetGroupId,
                AllocationTypeId = 1,
                ApprovedAmount = approvedAmount,
                RemainingBalance = approvedAmount,
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                Remarks = "Test Allocation"
            };

        public static CreateBudgetAllocationCommand ValidCreateCommand(
            List<CreateBudgetAllocationDto>? dtos = null) =>
            new CreateBudgetAllocationCommand
            {
                createBudgetAllocations = dtos ?? new List<CreateBudgetAllocationDto> { ValidCreateDto() }
            };

        public static UpsertBudgetAllocationOnApprovalCommand ValidUpsertCommand(int budgetRequestId = 1) =>
            new UpsertBudgetAllocationOnApprovalCommand { BudgetRequestId = budgetRequestId };

        public static BudgetManagement.Domain.Entities.BudgetAllocation ValidEntity(int id = 1) =>
            new BudgetManagement.Domain.Entities.BudgetAllocation
            {
                Id = id,
                FinancialYearId = 1,
                RequestById = 1,
                RequestMonthId = 1,
                UnitId = 1,
                BudgetGroupId = 1,
                AllocationTypeId = 1,
                ApprovedAmount = 50000m,
                RemainingBalance = 50000m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
