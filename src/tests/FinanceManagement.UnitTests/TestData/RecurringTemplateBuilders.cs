using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate;

namespace FinanceManagement.UnitTests.TestData
{
    internal static class RecurringTemplateBuilders
    {
        public static RecurringTemplateLineInputDto Line(int glAccountId = 5400101, decimal dr = 150000m, decimal cr = 0m) =>
            new()
            {
                GlAccountId = glAccountId,
                DrAmount = dr,
                CrAmount = cr,
                CostCentreId = 1,
                ProfitCentreId = 1,
                LineNarration = "Template line"
            };

        public static CreateRecurringJournalTemplateCommand ValidCreateCommand(
            string name = "Monthly Rent — Silvassa",
            List<RecurringTemplateLineInputDto>? lines = null) =>
            new()
            {
                TemplateName = name,
                VoucherTypeId = 1,
                FrequencyId = 141,
                StartDate = new DateOnly(2026, 4, 1),
                EndDate = null,
                AutoPost = true,
                AmountAdjustmentRuleId = 151,
                LowRisk = true,
                Lines = lines ?? new List<RecurringTemplateLineInputDto>
                {
                    Line(5400101, 150000m, 0m),
                    Line(2200105, 0m, 150000m)
                }
            };

        public static UpdateRecurringJournalTemplateCommand ValidUpdateCommand(
            int id = 1,
            string name = "Monthly Rent — Silvassa (edited)",
            int isActive = 1,
            List<RecurringTemplateLineInputDto>? lines = null) =>
            new()
            {
                Id = id,
                TemplateName = name,
                VoucherTypeId = 1,
                FrequencyId = 141,
                StartDate = new DateOnly(2026, 4, 1),
                EndDate = new DateOnly(2027, 3, 31),
                AutoPost = false,
                AmountAdjustmentRuleId = 151,
                LowRisk = false,
                IsActive = isActive,
                Lines = lines ?? new List<RecurringTemplateLineInputDto>
                {
                    Line(5400101, 160000m, 0m),
                    Line(2200105, 0m, 160000m)
                }
            };

        public static RecurringJournalTemplateHeaderDto ValidDto(int id = 1, string name = "Monthly Rent — Silvassa") =>
            new()
            {
                Id = id,
                TemplateName = name,
                VoucherTypeId = 1,
                VoucherTypeCode = "JV",
                VoucherTypeName = "Journal Voucher",
                FrequencyId = 141,
                FrequencyName = "Monthly",
                StartDate = new DateOnly(2026, 4, 1),
                AutoPost = true,
                AmountAdjustmentRuleId = 151,
                AmountAdjustmentRuleName = "Fixed",
                LowRisk = true,
                IsActive = true,
                IsDeleted = false,
                Lines = new List<RecurringJournalTemplateDetailDto>
                {
                    new() { Id = 1, LineNo = 1, GlAccountId = 5400101, DrAmount = 150000m },
                    new() { Id = 2, LineNo = 2, GlAccountId = 2200105, CrAmount = 150000m }
                }
            };

        public static List<RecurringJournalTemplateLookupDto> ValidLookupList() =>
            new() { new RecurringJournalTemplateLookupDto { Id = 1, TemplateName = "Monthly Rent — Silvassa" } };

        public static FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                TemplateName = "Monthly Rent — Silvassa",
                VoucherTypeId = 1,
                FrequencyId = 141,
                StartDate = new DateOnly(2026, 4, 1),
                AutoPost = true,
                AmountAdjustmentRuleId = 151,
                LowRisk = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
