using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;

namespace FinanceManagement.UnitTests.TestData
{
    /// <summary>
    /// US-GL03-01 — Builders that return valid baseline objects;
    /// individual tests tweak one or two fields to drive each assertion.
    /// </summary>
    internal static class FinancialYearMasterBuilders
    {
        public static CreateFinancialYearMasterCommand ValidCreateCommand(
            string code = "2024-25",
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            bool isTransitionYear = false) =>
            new()
            {
                FinancialYearCode = code,
                StartDate = startDate ?? new DateOnly(2024, 4, 1),
                EndDate   = endDate   ?? new DateOnly(2025, 3, 31),
                IsTransitionYear = isTransitionYear
            };

        public static UpdateFinancialYearMasterCommand ValidUpdateCommand(
            int id = 1,
            string code = "2024-25",
            int isActive = 1) =>
            new()
            {
                Id = id,
                FinancialYearCode = code,
                IsActive = isActive
            };

        public static FinancialYearMasterDto ValidDto(int id = 1, int companyId = 1) =>
            new()
            {
                Id = id,
                CompanyId = companyId,
                CompanyName = "Test Company",
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                StatusId = 100,
                StatusCode = "OPEN",
                StatusName = "Open",
                IsTransitionYear = false,
                IsActive = true,
                IsDeleted = false,
                Periods = new List<FinancialPeriodMasterDto>()
            };

        public static FinancialPeriodMasterDto ValidPeriodDto(
            int id = 1,
            int financialYearId = 1,
            byte periodNumber = 1,
            bool isAdjustment = false) =>
            new()
            {
                Id = id,
                FinancialYearId = financialYearId,
                FinancialYearCode = "2024-25",
                CompanyId = 1,
                PeriodNumber = periodNumber,
                PeriodName = isAdjustment ? "Adj-2024-25" : "Apr-2024",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate   = new DateOnly(2024, 4, 30),
                StatusId = 200,
                StatusCode = "OPEN",
                StatusName = "Open",
                IsAdjustmentPeriod = isAdjustment
            };

        public static FinancialYearMasterLookupDto ValidLookupDto(int id = 1) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                StatusId = 100,
                StatusCode = "OPEN"
            };
    }
}
