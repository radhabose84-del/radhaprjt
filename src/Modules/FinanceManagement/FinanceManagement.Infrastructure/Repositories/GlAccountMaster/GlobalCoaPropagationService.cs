using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.Common.Options;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountMaster
{
    // US-GL02-10 Multi-Company COA. Per-company copies of the global template are kept in sync here.
    // Writes go through ApplicationDbContext so audit fields + the COA-freeze trigger still apply (a copy
    // cannot be created/updated into a frozen target company — the trigger rolls it back as designed).
    public class GlobalCoaPropagationService : IGlobalCoaPropagationService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICompanyLookup _companyLookup;
        private readonly int _templateCompanyId;

        public GlobalCoaPropagationService(
            ApplicationDbContext db,
            ICompanyLookup companyLookup,
            IOptions<MultiCompanyCoaOptions> options)
        {
            _db = db;
            _companyLookup = companyLookup;
            _templateCompanyId = options.Value.TemplateCompanyId;
        }

        public async Task<int> InheritForCompanyAsync(int targetCompanyId, CancellationToken ct)
        {
            if (_templateCompanyId <= 0 || targetCompanyId == _templateCompanyId)
                return 0;

            var templates = await _db.GlAccountMaster
                .Where(a => a.CompanyId == _templateCompanyId
                            && a.IsGlobal
                            && !a.IsCompanyRestricted
                            && a.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync(ct);

            if (templates.Count == 0)
                return 0;

            var existingLinks = await _db.GlAccountMaster
                .Where(a => a.CompanyId == targetCompanyId && a.GlobalAccountId != null && a.IsDeleted == IsDelete.NotDeleted)
                .Select(a => a.GlobalAccountId!.Value)
                .ToListAsync(ct);
            var existingCodes = await _db.GlAccountMaster
                .Where(a => a.CompanyId == targetCompanyId && a.IsDeleted == IsDelete.NotDeleted)
                .Select(a => a.AccountCode)
                .ToListAsync(ct);

            var linkSet = existingLinks.ToHashSet();
            var codeSet = existingCodes.Where(c => c != null).Select(c => c!).ToHashSet();

            var created = 0;
            foreach (var template in templates)
            {
                if (linkSet.Contains(template.Id) || (template.AccountCode != null && codeSet.Contains(template.AccountCode)))
                    continue;

                _db.GlAccountMaster.Add(NewCopy(template, targetCompanyId));
                created++;
            }

            if (created > 0)
                await _db.SaveChangesAsync(ct);

            return created;
        }

        public async Task<int> FanOutNewGlobalAsync(int globalAccountId, CancellationToken ct)
        {
            if (_templateCompanyId <= 0)
                return 0;

            var template = await _db.GlAccountMaster
                .FirstOrDefaultAsync(a => a.Id == globalAccountId && a.IsDeleted == IsDelete.NotDeleted, ct);

            if (template is null || !template.IsGlobal || template.IsCompanyRestricted || template.CompanyId != _templateCompanyId)
                return 0;

            var siblingCompanyIds = await GetSiblingCompanyIdsAsync();
            if (siblingCompanyIds.Count == 0)
                return 0;

            // Companies that already have this template (by link) or a clashing code are skipped.
            var alreadyLinked = await _db.GlAccountMaster
                .Where(a => a.GlobalAccountId == globalAccountId && a.IsDeleted == IsDelete.NotDeleted)
                .Select(a => a.CompanyId)
                .ToListAsync(ct);
            var clashingCompanies = await _db.GlAccountMaster
                .Where(a => a.AccountCode == template.AccountCode
                            && siblingCompanyIds.Contains(a.CompanyId)
                            && a.IsDeleted == IsDelete.NotDeleted)
                .Select(a => a.CompanyId)
                .ToListAsync(ct);

            var skip = alreadyLinked.Concat(clashingCompanies).ToHashSet();

            var created = 0;
            foreach (var companyId in siblingCompanyIds)
            {
                if (skip.Contains(companyId))
                    continue;

                _db.GlAccountMaster.Add(NewCopy(template, companyId));
                created++;
            }

            if (created > 0)
                await _db.SaveChangesAsync(ct);

            return created;
        }

        public async Task<int> PropagateUpdateAsync(int globalAccountId, CancellationToken ct)
        {
            if (_templateCompanyId <= 0)
                return 0;

            var template = await _db.GlAccountMaster
                .FirstOrDefaultAsync(a => a.Id == globalAccountId && a.IsDeleted == IsDelete.NotDeleted, ct);

            if (template is null || !template.IsGlobal || template.CompanyId != _templateCompanyId)
                return 0;

            // Only copies that have NOT been locally overridden track the global template (AC3).
            var copies = await _db.GlAccountMaster
                .Where(a => a.GlobalAccountId == globalAccountId
                            && !a.IsLocalOverride
                            && a.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync(ct);

            if (copies.Count == 0)
                return 0;

            foreach (var copy in copies)
            {
                // AccountCode + CompanyId stay; everything else mirrors the template. AccountGroupId is
                // shared (group hierarchy uses a global GroupCode), so the same id is valid per company.
                copy.AccountTypeId = template.AccountTypeId;
                copy.AccountGroupId = template.AccountGroupId;
                copy.AccountName = template.AccountName;
                copy.Description = template.Description;
                copy.NormalBalanceId = template.NormalBalanceId;
                copy.CurrencyTypeId = template.CurrencyTypeId;
                copy.SubLedgerTypeId = template.SubLedgerTypeId;
                copy.IsCostCentreMandatory = template.IsCostCentreMandatory;
                copy.IsProfitCentreMandatory = template.IsProfitCentreMandatory;
                copy.IsTaxRelevant = template.IsTaxRelevant;
                copy.IsInterCompany = template.IsInterCompany;
                copy.IsReconciliationRequired = template.IsReconciliationRequired;
                copy.IsActive = template.IsActive;
            }

            await _db.SaveChangesAsync(ct);
            return copies.Count;
        }

        private async Task<List<int>> GetSiblingCompanyIdsAsync()
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            var template = companies.FirstOrDefault(c => c.CompanyId == _templateCompanyId);
            if (template is null)
                return new List<int>();

            return companies
                .Where(c => c.EntityId == template.EntityId && c.CompanyId != _templateCompanyId)
                .Select(c => c.CompanyId)
                .ToList();
        }

        private static Domain.Entities.GlAccountMaster NewCopy(Domain.Entities.GlAccountMaster template, int targetCompanyId) =>
            new()
            {
                CompanyId = targetCompanyId,
                GlobalAccountId = template.Id,
                IsGlobal = false,
                IsLocalOverride = false,
                IsCompanyRestricted = false,
                AccountTypeId = template.AccountTypeId,
                AccountGroupId = template.AccountGroupId,
                AccountCode = template.AccountCode,
                AccountName = template.AccountName,
                Description = template.Description,
                NormalBalanceId = template.NormalBalanceId,
                CurrencyTypeId = template.CurrencyTypeId,
                SubLedgerTypeId = template.SubLedgerTypeId,
                IsCostCentreMandatory = template.IsCostCentreMandatory,
                IsProfitCentreMandatory = template.IsProfitCentreMandatory,
                IsTaxRelevant = template.IsTaxRelevant,
                IsInterCompany = template.IsInterCompany,
                IsReconciliationRequired = template.IsReconciliationRequired,
                IsActive = template.IsActive,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
