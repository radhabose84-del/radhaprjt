using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.BudgetRequest.Commands.Delete;
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.TestData
{
    public static class BudgetRequestBuilders
    {
        public static CreateBudgetRequestCommand ValidCreateCommand(
            int unitId = 1,
            int currencyId = 1,
            int requestTypeId = 1,
            decimal requestAmount = 10000m,
            int financialYearId = 1) =>
            new CreateBudgetRequestCommand
            {
                UnitId = unitId,
                CurrencyId = currencyId,
                RequestTypeId = requestTypeId,
                RequestAmount = requestAmount,
                FinancialYearId = financialYearId,
                BudgetGroupId = 1,
                RequestById = 1,
                Remarks = "Test Request",
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1))
            };

        public static UpdateBudgetRequestCommand ValidUpdateCommand(
            int id = 1,
            int unitId = 1,
            int currencyId = 1,
            int requestTypeId = 1,
            decimal requestAmount = 15000m,
            int editFlag = 0) =>
            new UpdateBudgetRequestCommand
            {
                Id = id,
                UnitId = unitId,
                CurrencyId = currencyId,
                RequestTypeId = requestTypeId,
                RequestAmount = requestAmount,
                EditFlag = editFlag,
                FinancialYearId = 1,
                BudgetGroupId = 1,
                Remarks = "Updated Request",
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1))
            };

        public static DeleteBudgetRequestCommand ValidDeleteCommand(int id = 1) =>
            new DeleteBudgetRequestCommand { Id = id };

        public static BudgetRequestDto ValidDto(int id = 1) =>
            new BudgetRequestDto
            {
                Id = id,
                RequestCode = "REQ-2025-001",
                UnitId = 1,
                CurrencyId = 1,
                RequestTypeId = 1,
                RequestAmount = 10000m,
                StatusId = 1,
                BudgetGroupId = 1
            };

        public static BudgetRequestListItemDto ValidListItemDto(int id = 1) =>
            new BudgetRequestListItemDto
            {
                Id = id,
                RequestCode = "REQ-2025-001",
                UnitId = 1,
                CurrencyId = 1,
                FinancialYearId = 1,
                RequestTypeId = 1,
                RequestAmount = 10000m,
                StatusId = 1,
                UnitName = "Test Unit",
                CurrencyName = "INR",
                FinancialYearName = "2025-26"
            };

        public static BudgetManagement.Domain.Entities.BudgetRequest ValidEntity(int id = 1) =>
            new BudgetManagement.Domain.Entities.BudgetRequest
            {
                Id = id,
                RequestCode = "REQ-2025-001",
                UnitId = 1,
                CurrencyId = 1,
                RequestTypeId = 1,
                RequestAmount = 10000m,
                StatusId = 1,
                FinancialYearId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
