using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.UpdateAccountingPeriod;
using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class AccountingPeriodBuilders
    {
        public static CreateAccountingPeriodCommand ValidCreateCommand(
            int financialYearId = 3,
            string name = "Jun 2026",
            int periodNo = 3,
            int statusId = 121) =>
            new()
            {
                FinancialYearId = financialYearId,
                PeriodName = name,
                PeriodNo = periodNo,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = statusId
            };

        public static UpdateAccountingPeriodCommand ValidUpdateCommand(
            int id = 1,
            string name = "Jun 2026",
            int statusId = 121,
            int isActive = 1) =>
            new()
            {
                Id = id,
                PeriodName = name,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = statusId,
                IsActive = isActive
            };

        public static AccountingPeriodDto ValidDto(
            int id = 1,
            string name = "Jun 2026",
            int periodNo = 3) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                CompanyName = "Test Company",
                FinancialYearId = 3,
                FinancialYearName = "2026-27",
                PeriodName = name,
                PeriodNo = periodNo,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = 121,
                StatusName = "Open",
                IsActive = true,
                IsDeleted = false
            };

        public static List<AccountingPeriodLookupDto> ValidLookupList() =>
            new()
            {
                new AccountingPeriodLookupDto { Id = 1, PeriodNo = 3, PeriodName = "Jun 2026" }
            };

        public static FinanceManagement.Domain.Entities.AccountingPeriod ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                FinancialYearId = 3,
                PeriodName = "Jun 2026",
                PeriodNo = 3,
                StartDate = new DateOnly(2026, 6, 1),
                EndDate = new DateOnly(2026, 6, 30),
                StatusId = 121,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
