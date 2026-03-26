using Microsoft.EntityFrameworkCore;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Infrastructure.Data;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Repositories.VehicleMovementRecord
{
    public class VehicleMovementRecordCommandRepository : IVehicleMovementRecordCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public VehicleMovementRecordCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<int> CreateAsync(Domain.Entities.VehicleMovementRecord entity, int transactionTypeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _applicationDbContext.VehicleMovementRecord.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    // Increment DocNo via Finance lookup (both in same transaction)
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId);

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

        public async Task<int> UpdateAsync(Domain.Entities.VehicleMovementRecord entity)
        {
            var existingEntity = await _applicationDbContext.VehicleMovementRecord
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Editable fields (until Gate Out)
            existingEntity.VehicleNumber = entity.VehicleNumber;
            existingEntity.DriverName = entity.DriverName;
            existingEntity.DriverLicenseNo = entity.DriverLicenseNo;
            existingEntity.DriverMobileNo = entity.DriverMobileNo;
            existingEntity.TransporterId = entity.TransporterId;
            existingEntity.PurposeOfVisitId = entity.PurposeOfVisitId;
            existingEntity.ReferenceDocTypeId = entity.ReferenceDocTypeId;
            existingEntity.ReferenceDocNo = entity.ReferenceDocNo;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.IsActive = entity.IsActive;

            _applicationDbContext.VehicleMovementRecord.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.VehicleMovementRecord
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.VehicleMovementRecord.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
