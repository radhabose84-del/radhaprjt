using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class JournalImportBuilders
    {
        public static List<JournalImportRowInputDto> ValidRows() => new()
        {
            new JournalImportRowInputDto
            {
                RowNo = 1, GroupNo = 1, VoucherTypeId = 1, VoucherDate = new DateOnly(2026, 6, 15),
                Narration = "Imported voucher", GlAccountId = 5400101, DrAmount = 1000m, CrAmount = 0m,
                CurrencyId = 1, CostCentreId = 1, ProfitCentreId = 1, LineNarration = "Dr", ReferenceDocNo = "IMP/1"
            },
            new JournalImportRowInputDto
            {
                RowNo = 2, GroupNo = 1, VoucherTypeId = 1, VoucherDate = new DateOnly(2026, 6, 15),
                Narration = "Imported voucher", GlAccountId = 2200105, DrAmount = 0m, CrAmount = 1000m,
                CurrencyId = 1, CostCentreId = null, ProfitCentreId = 1, LineNarration = "Cr", ReferenceDocNo = "IMP/1"
            }
        };

        public static ImportJournalsCommand ValidCommand(string fileName = "June_Accruals.xlsx", List<JournalImportRowInputDto>? rows = null) =>
            new() { FileName = fileName, Rows = rows ?? ValidRows() };
    }
}
