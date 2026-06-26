using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.Journal;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.RecurringJournalTemplate
{
    internal sealed class RecurringSeededIds
    {
        public int VoucherTypeId;
        public int GlAccountDrId;
        public int GlAccountCrId;
        public int CostCentreId;
        public int ProfitCentreId;
        public int FrequencyId;
        public int AmountAdjustmentRuleId;
        public int CurrencyId;
    }

    internal static class RecurringTemplateSeed
    {
        // Reuses the Journal FK graph (voucher type, GL accounts, cost/profit centre) and adds the
        // RECURRING_FREQUENCY + AMOUNT_ADJ_RULE MiscMaster rows the template references.
        public static async Task<RecurringSeededIds> SeedAsync(DbFixture fixture)
        {
            var baseIds = await JournalTestSeed.SeedGraphAsync(fixture);

            await using var ctx = fixture.CreateFreshDbContext();

            var freqType = new MiscTypeMaster { MiscTypeCode = "RECURRING_FREQUENCY", Description = "Frequency", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var adjType = new MiscTypeMaster { MiscTypeCode = "AMOUNT_ADJ_RULE", Description = "Amount Adjustment Rule", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscTypeMaster.AddRange(freqType, adjType);
            await ctx.SaveChangesAsync();

            var monthly = new MiscMaster { MiscTypeId = freqType.Id, Code = "MONTHLY", Description = "Monthly", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            var fixed_ = new MiscMaster { MiscTypeId = adjType.Id, Code = "FIXED", Description = "Fixed", SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
            ctx.MiscMaster.AddRange(monthly, fixed_);
            await ctx.SaveChangesAsync();

            return new RecurringSeededIds
            {
                VoucherTypeId = baseIds.VoucherTypeId,
                GlAccountDrId = baseIds.GlAccountDrId,
                GlAccountCrId = baseIds.GlAccountCrId,
                CostCentreId = baseIds.CostCentreId,
                ProfitCentreId = baseIds.ProfitCentreId,
                FrequencyId = monthly.Id,
                AmountAdjustmentRuleId = fixed_.Id,
                CurrencyId = baseIds.CurrencyId
            };
        }

        public static FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader BuildTemplate(RecurringSeededIds ids, string name = "Monthly Rent — Silvassa")
        {
            return new FinanceManagement.Domain.Entities.RecurringJournalTemplateHeader
            {
                TemplateName = name,
                CompanyId = 1,
                UnitId = 1,
                VoucherTypeId = ids.VoucherTypeId,
                FrequencyId = ids.FrequencyId,
                StartDate = new DateOnly(2026, 4, 1),
                EndDate = null,
                AutoPost = true,
                AmountAdjustmentRuleId = ids.AmountAdjustmentRuleId,
                LowRisk = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Lines = new List<RecurringJournalTemplateDetail>
                {
                    new()
                    {
                        LineNo = 1, GlAccountId = ids.GlAccountDrId, DrAmount = 150000m, CrAmount = 0m,
                        CurrencyId = ids.CurrencyId, ExchangeRate = 1m,
                        CostCentreId = ids.CostCentreId, ProfitCentreId = ids.ProfitCentreId, LineNarration = "Rent",
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    },
                    new()
                    {
                        LineNo = 2, GlAccountId = ids.GlAccountCrId, DrAmount = 0m, CrAmount = 150000m,
                        CurrencyId = ids.CurrencyId, ExchangeRate = 1m,
                        CostCentreId = null, ProfitCentreId = ids.ProfitCentreId, LineNarration = "Rent payable",
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
        }
    }
}
