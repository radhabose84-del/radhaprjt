using FinanceManagement.Domain.Entities;
using FinanceManagement.IntegrationTests.Common;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.Journal
{
    // Seeds the full FK graph needed to create + post a journal voucher against the real DB.
    internal sealed class SeededIds
    {
        public int CompanyId = 1;
        public int FinancialYearId = 3;
        public int StatusDraftId;
        public int StatusPostedId;
        public int SourceManualId;
        public int VoucherTypeId;
        public int AccountingPeriodId;
        public int GlAccountDrId;
        public int GlAccountCrId;
        public int CostCentreId;
        public int ProfitCentreId;
        public int CurrencyId;
    }

    internal static class JournalTestSeed
    {
        public static async Task<SeededIds> SeedGraphAsync(DbFixture fixture)
        {
            await using var ctx = fixture.CreateFreshDbContext();
            var ids = new SeededIds();

            // --- MiscType + MiscMaster lookups ---
            var journalStatusType = NewType("JOURNAL_STATUS");
            var journalSourceType = NewType("JOURNAL_SOURCE");
            var periodStatusType = NewType("PERIOD_STATUS");
            var glMiscType = NewType("JV_TEST_MISC");
            ctx.MiscTypeMaster.AddRange(journalStatusType, journalSourceType, periodStatusType, glMiscType);
            await ctx.SaveChangesAsync();

            var draft = NewMisc(journalStatusType.Id, "DRAFT");
            var posted = NewMisc(journalStatusType.Id, "POSTED");
            var manual = NewMisc(journalSourceType.Id, "MANUAL");
            var recurring = NewMisc(journalSourceType.Id, "RECURRING");
            var open = NewMisc(periodStatusType.Id, "OPEN");
            var normalBalance = NewMisc(glMiscType.Id, "NB");
            var subLedger = NewMisc(glMiscType.Id, "SLT");
            var ccLevel = NewMisc(glMiscType.Id, "CCL");
            var pcLevel = NewMisc(glMiscType.Id, "PCL");
            ctx.MiscMaster.AddRange(draft, posted, manual, recurring, open, normalBalance, subLedger, ccLevel, pcLevel);
            await ctx.SaveChangesAsync();

            ids.StatusDraftId = draft.Id;
            ids.StatusPostedId = posted.Id;
            ids.SourceManualId = manual.Id;

            // --- Account type / group / currency ---
            var accountType = new AccountTypeMaster
            {
                CompanyId = 1, AccountTypeName = "Expense", StartCode = "5", AccountCodeLength = 7, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            var accountGroup = new FinanceManagement.Domain.Entities.AccountGroup
            {
                CompanyId = 1, GroupCode = "JVGRP", GroupName = "JV Test Group", Level = 1, IsLeaf = true, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            var currency = new FinanceManagement.Domain.Entities.CurrencyForexConfig
            {
                CompanyId = 1, CurrencyTypeCode = "INR", CurrencyTypeName = "Indian Rupee",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.AccountTypeMaster.Add(accountType);
            ctx.AccountGroup.Add(accountGroup);
            ctx.CurrencyForexConfig.Add(currency);
            await ctx.SaveChangesAsync();
            ids.CurrencyId = currency.Id;

            // --- GL accounts (debit P&L + credit BS), cost / profit centre ---
            var glDr = NewGlAccount("5200101", "Salaries & Wages", accountType.Id, accountGroup.Id, normalBalance.Id, currency.Id, subLedger.Id);
            var glCr = NewGlAccount("2200101", "Salary Payable", accountType.Id, accountGroup.Id, normalBalance.Id, currency.Id, subLedger.Id);
            ctx.GlAccountMaster.AddRange(glDr, glCr);

            var costCentre = new FinanceManagement.Domain.Entities.CostCentre
            {
                UnitId = 1, CompanyId = 1, CostCentreCode = "SPIN-001", CostCentreName = "Ring Spinning",
                CentreLevelId = ccLevel.Id, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            var profitCentre = new FinanceManagement.Domain.Entities.ProfitCentre
            {
                CompanyId = 1, ProfitCentreCode = "PC-YARN", ProfitCentreName = "Yarn Division",
                LevelId = pcLevel.Id, IsRevenueLinked = true, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.CostCentre.Add(costCentre);
            ctx.ProfitCentre.Add(profitCentre);

            // --- Voucher type + accounting period ---
            var voucherType = new VoucherTypeMaster
            {
                CompanyId = 1, VoucherTypeCode = "JV", VoucherTypeName = "Journal Voucher", NumberPadding = 4, IsSystem = false,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            var period = new FinanceManagement.Domain.Entities.AccountingPeriod
            {
                CompanyId = 1, FinancialYearId = 3, PeriodName = "Jun 2026", PeriodNo = 3,
                StartDate = new DateOnly(2026, 6, 1), EndDate = new DateOnly(2026, 6, 30), StatusId = open.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            ctx.VoucherTypeMaster.Add(voucherType);
            ctx.AccountingPeriod.Add(period);
            await ctx.SaveChangesAsync();

            ids.GlAccountDrId = glDr.Id;
            ids.GlAccountCrId = glCr.Id;
            ids.CostCentreId = costCentre.Id;
            ids.ProfitCentreId = profitCentre.Id;
            ids.VoucherTypeId = voucherType.Id;
            ids.AccountingPeriodId = period.Id;

            return ids;
        }

        public static FinanceManagement.Domain.Entities.JournalHeader BuildDraftJournal(SeededIds ids, decimal amount = 1000m)
        {
            return new FinanceManagement.Domain.Entities.JournalHeader
            {
                CompanyId = ids.CompanyId,
                UnitId = 1,
                VoucherTypeId = ids.VoucherTypeId,
                VoucherDate = new DateOnly(2026, 6, 15),
                FinancialYearId = ids.FinancialYearId,
                AccountingPeriodId = ids.AccountingPeriodId,
                Narration = "Salary booking — June",
                StatusId = ids.StatusDraftId,
                SourceId = ids.SourceManualId,
                IsReversal = false,
                AutoApproved = false,
                TotalDr = amount,
                TotalCr = amount,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = new List<JournalDetail>
                {
                    new()
                    {
                        LineNo = 1, GlAccountId = ids.GlAccountDrId, DrAmount = amount, CrAmount = 0m,
                        CurrencyId = ids.CurrencyId, ExchangeRate = 1m, BaseDrAmount = amount, BaseCrAmount = 0m,
                        CostCentreId = ids.CostCentreId, ProfitCentreId = ids.ProfitCentreId,
                        LineNarration = "Salaries", ReferenceDocNo = "PAY/JUN/2026",
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    },
                    new()
                    {
                        LineNo = 2, GlAccountId = ids.GlAccountCrId, DrAmount = 0m, CrAmount = amount,
                        CurrencyId = ids.CurrencyId, ExchangeRate = 1m, BaseDrAmount = 0m, BaseCrAmount = amount,
                        CostCentreId = null, ProfitCentreId = ids.ProfitCentreId,
                        LineNarration = "Salary payable", ReferenceDocNo = "PAY/JUN/2026",
                        IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                    }
                }
            };
        }

        private static MiscTypeMaster NewType(string code) => new()
        {
            MiscTypeCode = code, Description = code, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
        };

        private static MiscMaster NewMisc(int typeId, string code) => new()
        {
            MiscTypeId = typeId, Code = code, Description = code, SortOrder = 1,
            IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
        };

        private static FinanceManagement.Domain.Entities.GlAccountMaster NewGlAccount(
            string code, string name, int accountTypeId, int accountGroupId, int normalBalanceId, int currencyId, int subLedgerId) => new()
        {
            CompanyId = 1, AccountTypeId = accountTypeId, AccountGroupId = accountGroupId,
            AccountCode = code, AccountName = name,
            NormalBalanceId = normalBalanceId, CurrencyTypeId = currencyId, SubLedgerTypeId = subLedgerId,
            IsCostCentreMandatory = false, IsTaxRelevant = false, IsInterCompany = false, IsReconciliationRequired = false,
            IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
        };
    }
}
