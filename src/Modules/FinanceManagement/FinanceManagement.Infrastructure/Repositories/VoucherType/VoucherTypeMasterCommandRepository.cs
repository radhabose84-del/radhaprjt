using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.VoucherType
{
    public class VoucherTypeMasterCommandRepository : IVoucherTypeMasterCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VoucherTypeMasterCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateAsync(VoucherTypeMaster entity, IEnumerable<int> accountTypeIds, int? initialFinancialYearId)
        {
            entity.AllowedAccountTypes = accountTypeIds
                .Distinct()
                .Select(id => new VoucherTypeAccountType { AccountTypeId = id })
                .ToList();

            if (initialFinancialYearId.HasValue && initialFinancialYearId.Value > 0)
            {
                entity.NumberSeries = new List<VoucherTypeNumberSeries>
                {
                    new VoucherTypeNumberSeries
                    {
                        FinancialYearId = initialFinancialYearId.Value,
                        LastUsedNumber = 0
                    }
                };
            }

            await _dbContext.VoucherTypeMaster.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateAsync(VoucherTypeMaster entity, IEnumerable<int> accountTypeIds)
        {
            var existing = await _dbContext.VoucherTypeMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // VoucherTypeCode is immutable — only mutable fields are updated.
            existing.VoucherTypeName = entity.VoucherTypeName;
            existing.NumberPadding = entity.NumberPadding;
            existing.IsActive = entity.IsActive;
            _dbContext.VoucherTypeMaster.Update(existing);

            // Reconcile allowed account types (soft-delete removed, add new)
            var requested = accountTypeIds.Distinct().ToList();

            var currentRows = await _dbContext.VoucherTypeAccountType
                .Where(x => x.VoucherTypeId == entity.Id && x.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();

            foreach (var row in currentRows.Where(r => !requested.Contains(r.AccountTypeId)))
            {
                row.IsDeleted = IsDelete.Deleted;
                _dbContext.VoucherTypeAccountType.Update(row);
            }

            var currentIds = currentRows.Select(r => r.AccountTypeId).ToHashSet();
            foreach (var accountTypeId in requested.Where(id => !currentIds.Contains(id)))
            {
                await _dbContext.VoucherTypeAccountType.AddAsync(new VoucherTypeAccountType
                {
                    VoucherTypeId = entity.Id,
                    AccountTypeId = accountTypeId
                });
            }

            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.VoucherTypeMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.VoucherTypeMaster.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> ResetSeriesAsync(int voucherTypeId, int financialYearId)
        {
            var existing = await _dbContext.VoucherTypeNumberSeries
                .FirstOrDefaultAsync(x => x.VoucherTypeId == voucherTypeId
                    && x.FinancialYearId == financialYearId
                    && x.IsDeleted == IsDelete.NotDeleted);

            if (existing != null)
            {
                existing.LastUsedNumber = 0;
                _dbContext.VoucherTypeNumberSeries.Update(existing);
                await _dbContext.SaveChangesAsync();
                return existing.Id;
            }

            var newRow = new VoucherTypeNumberSeries
            {
                VoucherTypeId = voucherTypeId,
                FinancialYearId = financialYearId,
                LastUsedNumber = 0
            };
            await _dbContext.VoucherTypeNumberSeries.AddAsync(newRow);
            await _dbContext.SaveChangesAsync();
            return newRow.Id;
        }
    }
}
