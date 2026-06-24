using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class JournalBuilders
    {
        public static JournalLineInputDto DrLine(int glAccountId = 5200101, decimal amount = 1000m, int? costCentreId = 1) =>
            new()
            {
                GlAccountId = glAccountId,
                DrAmount = amount,
                CrAmount = 0m,
                CurrencyId = 1,
                CostCentreId = costCentreId,
                ProfitCentreId = 1,
                LineNarration = "Debit line",
                ReferenceDocNo = "REF/001"
            };

        public static JournalLineInputDto CrLine(int glAccountId = 2200101, decimal amount = 1000m, int? costCentreId = null) =>
            new()
            {
                GlAccountId = glAccountId,
                DrAmount = 0m,
                CrAmount = amount,
                CurrencyId = 1,
                CostCentreId = costCentreId,
                ProfitCentreId = 1,
                LineNarration = "Credit line",
                ReferenceDocNo = "REF/001"
            };

        public static CreateJournalCommand ValidCreateCommand(
            int voucherTypeId = 1,
            string narration = "Salary booking — June",
            List<JournalLineInputDto>? lines = null) =>
            new()
            {
                VoucherTypeId = voucherTypeId,
                VoucherDate = new DateOnly(2026, 6, 15),
                Narration = narration,
                Lines = lines ?? new List<JournalLineInputDto> { DrLine(), CrLine() }
            };

        public static UpdateJournalCommand ValidUpdateCommand(
            int id = 1,
            int voucherTypeId = 1,
            List<JournalLineInputDto>? lines = null) =>
            new()
            {
                Id = id,
                VoucherTypeId = voucherTypeId,
                VoucherDate = new DateOnly(2026, 6, 15),
                Narration = "Salary booking — June (edited)",
                Lines = lines ?? new List<JournalLineInputDto> { DrLine(), CrLine() }
            };

        public static JournalHeaderDto ValidDto(int id = 1, string? voucherNo = null) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                VoucherTypeId = 1,
                VoucherTypeCode = "JV",
                VoucherTypeName = "Journal Voucher",
                VoucherNo = voucherNo,
                VoucherDate = new DateOnly(2026, 6, 15),
                FinancialYearId = 3,
                FinancialYearName = "2026-27",
                AccountingPeriodId = 4,
                PeriodName = "Jun 2026",
                Narration = "Salary booking — June",
                StatusId = 101,
                StatusName = "Draft",
                SourceId = 111,
                SourceName = "Manual",
                TotalDr = 1000m,
                TotalCr = 1000m,
                IsActive = true,
                IsDeleted = false,
                Lines = new List<JournalDetailDto>
                {
                    new() { Id = 1, LineNo = 1, GlAccountId = 5200101, DrAmount = 1000m, CurrencyId = 1, CostCentreId = 1 },
                    new() { Id = 2, LineNo = 2, GlAccountId = 2200101, CrAmount = 1000m, CurrencyId = 1 }
                }
            };

        public static PostJournalResultDto ValidPostResult(string voucherNo = "JV/2026-27/0001") =>
            new()
            {
                JournalId = 1,
                VoucherNo = voucherNo,
                UpdatedBalances = new List<PostedBalanceDto>
                {
                    new() { GlAccountId = 5200101, CostCentreId = 1, Balance = 1000m },
                    new() { GlAccountId = 2200101, CostCentreId = null, Balance = -1000m }
                }
            };

        public static FinanceManagement.Domain.Entities.JournalHeader ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                CompanyId = 1,
                VoucherTypeId = 1,
                VoucherDate = new DateOnly(2026, 6, 15),
                FinancialYearId = 3,
                AccountingPeriodId = 4,
                StatusId = 101,
                SourceId = 111,
                TotalDr = 1000m,
                TotalCr = 1000m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
