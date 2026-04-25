using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.TripSheet
{
    public class TripSheetCommandRepository : ITripSheetCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public TripSheetCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(TripSheetHeader entity, int typeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Detach details from header for separate insertion
                    var details = entity.TripSheetDetails?.ToList();
                    entity.TripSheetDetails = null;

                    // Insert header
                    await _applicationDbContext.TripSheetHeader.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert details
                    if (details != null && details.Count > 0)
                    {
                        foreach (var detail in details)
                        {
                            detail.TripSheetHeaderId = entity.Id;
                            await _applicationDbContext.TripSheetDetail.AddAsync(detail);
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

                    // Increment document sequence number
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(typeId, dbConnection, dbTransaction);

                    await _applicationDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return entity.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<int> UpdateAsync(TripSheetHeader entity, List<TripSheetDetail> newDetails)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _applicationDbContext.TripSheetHeader
                        .Include(h => h.TripSheetDetails)
                        .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

                    if (existing == null)
                        return 0;

                    // Update header fields
                    existing.TripDate = entity.TripDate;
                    existing.VehicleNo = entity.VehicleNo;
                    existing.UnitId = entity.UnitId;
                    existing.Remarks = entity.Remarks;
                    existing.IsActive = entity.IsActive;

                    // Remove old details
                    if (existing.TripSheetDetails != null)
                    {
                        _applicationDbContext.TripSheetDetail.RemoveRange(existing.TripSheetDetails);
                    }
                    await _applicationDbContext.SaveChangesAsync();

                    // Insert new details
                    if (newDetails.Count > 0)
                    {
                        foreach (var detail in newDetails)
                        {
                            detail.TripSheetHeaderId = entity.Id;
                            await _applicationDbContext.TripSheetDetail.AddAsync(detail);
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

                    _applicationDbContext.TripSheetHeader.Update(existing);
                    await _applicationDbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return existing.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.TripSheetHeader
                .Include(h => h.TripSheetDetails)
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    // Soft delete header
                    existing.IsDeleted = IsDelete.Deleted;
                    _applicationDbContext.TripSheetHeader.Update(existing);
                    await _applicationDbContext.SaveChangesAsync(ct);

                    await transaction.CommitAsync(ct);
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}
