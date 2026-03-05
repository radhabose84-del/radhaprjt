using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.StoHeader
{
    public class StoHeaderCommandRepository : IStoHeaderCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public StoHeaderCommandRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> GenerateNextStoNumberAsync(int supplyingPlantId, CancellationToken ct = default)
        {
            var prefix = $"STO-{supplyingPlantId}-";

            var lastNumber = await _dbContext.StoHeader
                .Where(x => x.StoNumber != null && x.StoNumber.StartsWith(prefix))
                .OrderByDescending(x => x.StoNumber)
                .Select(x => x.StoNumber)
                .FirstOrDefaultAsync(ct);

            var nextSeq = 1;
            if (lastNumber != null)
            {
                var seqPart = lastNumber.Substring(prefix.Length);
                if (int.TryParse(seqPart, out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(Domain.Entities.StoHeader entity)
        {
            // Separate details from header
            var details = entity.StoDetails?.ToList();
            entity.StoDetails = null;

            // Save header first to get auto-generated Id
            await _dbContext.StoHeader.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            // Insert details with header FK and default LineStatus
            if (details != null && details.Count > 0)
            {
                // Fetch "Open" line status from MiscMaster (MiscType = "StoLineItemStatus")
                var openStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "StoLineItemStatus" &&
                        m.Code == "Open" &&
                        m.IsActive == Status.Active &&
                        m.IsDeleted == IsDelete.NotDeleted);

                foreach (var detail in details)
                {
                    detail.StoHeaderId = entity.Id;
                    detail.LineStatusId = openStatus?.Id;
                    await _dbContext.StoDetail.AddAsync(detail);
                }

                await _dbContext.SaveChangesAsync();
            }

            return entity.Id;
        }

        public async Task<int> UpdateAsync(Domain.Entities.StoHeader entity)
        {
            var existing = await _dbContext.StoHeader
                .Include(h => h.StoDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Update header fields (StoNumber excluded — immutable)
            existing.DocumentDate = entity.DocumentDate;
            existing.ExpectedDeliveryDate = entity.ExpectedDeliveryDate;
            existing.StoTypeId = entity.StoTypeId;
            existing.MovementTypeId = entity.MovementTypeId;
            existing.SupplyingPlantId = entity.SupplyingPlantId;
            existing.SupplyingStorageLocationId = entity.SupplyingStorageLocationId;
            existing.ReceivingPlantId = entity.ReceivingPlantId;
            existing.ReceivingStorageLocationId = entity.ReceivingStorageLocationId;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            // Delete all old details and re-insert new ones
            if (existing.StoDetails != null && existing.StoDetails.Count > 0)
            {
                _dbContext.StoDetail.RemoveRange(existing.StoDetails);
            }

            if (entity.StoDetails != null && entity.StoDetails.Count > 0)
            {
                // Fetch "Open" line status
                var openStatus = await _dbContext.MiscMaster
                    .Include(m => m.MiscTypeMaster)
                    .FirstOrDefaultAsync(m =>
                        m.MiscTypeMaster != null &&
                        m.MiscTypeMaster.MiscTypeCode == "StoLineItemStatus" &&
                        m.Code == "Open" &&
                        m.IsActive == Status.Active &&
                        m.IsDeleted == IsDelete.NotDeleted);

                foreach (var detail in entity.StoDetails)
                {
                    detail.StoHeaderId = existing.Id;
                    detail.LineStatusId = openStatus?.Id;
                    await _dbContext.StoDetail.AddAsync(detail);
                }
            }

            _dbContext.StoHeader.Update(existing);
            await _dbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _dbContext.StoHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _dbContext.StoHeader.Update(existing);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
