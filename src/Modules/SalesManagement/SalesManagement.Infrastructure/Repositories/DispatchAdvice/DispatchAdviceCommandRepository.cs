using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.DispatchAdvice
{
    public class DispatchAdviceCommandRepository : IDispatchAdviceCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public DispatchAdviceCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<string> GenerateNextDispatchNoAsync(int unitId, CancellationToken ct = default)
        {
            var prefix = $"DA-{unitId}-";

            var lastDispatch = await _applicationDbContext.DispatchAdviceHeader
                .Where(x => x.DispatchNo != null && x.DispatchNo.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.DispatchNo)
                .FirstOrDefaultAsync(ct);

            int nextSeq = 1;
            if (lastDispatch != null)
            {
                var parts = lastDispatch.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(DispatchAdviceHeader entity)
        {
            // Separate details from header
            var details = entity.DispatchAdviceDetails?.ToList();
            entity.DispatchAdviceDetails = null;

            // Insert header
            await _applicationDbContext.DispatchAdviceHeader.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();

            // Insert details
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    detail.DispatchAdviceHeaderId = entity.Id;
                    await _applicationDbContext.DispatchAdviceDetail.AddAsync(detail);
                }
                await _applicationDbContext.SaveChangesAsync();
            }

            return entity.Id;
        }

        public async Task<int> UpdateAsync(DispatchAdviceHeader entity)
        {
            var existingEntity = await _applicationDbContext.DispatchAdviceHeader
                .Include(h => h.DispatchAdviceDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Update header fields (DispatchNo and StatusId are immutable via update)
            existingEntity.DispatchDate = entity.DispatchDate;
            existingEntity.SalesOrderId = entity.SalesOrderId;
            existingEntity.PartyId = entity.PartyId;
            existingEntity.TotOrderQty = entity.TotOrderQty;
            existingEntity.TotDispatchedQty = entity.TotDispatchedQty;
            existingEntity.TotPendingQty = entity.TotPendingQty;
            existingEntity.DispatchAddressId = entity.DispatchAddressId;
            existingEntity.TransporterId = entity.TransporterId;
            existingEntity.VehicleNo = entity.VehicleNo;
            existingEntity.DriverName = entity.DriverName;
            existingEntity.LRNo = entity.LRNo;
            existingEntity.IsActive = entity.IsActive;

            // Remove existing details
            if (existingEntity.DispatchAdviceDetails != null && existingEntity.DispatchAdviceDetails.Any())
            {
                _applicationDbContext.DispatchAdviceDetail.RemoveRange(existingEntity.DispatchAdviceDetails);
            }

            // Insert new details
            if (entity.DispatchAdviceDetails != null && entity.DispatchAdviceDetails.Any())
            {
                foreach (var detail in entity.DispatchAdviceDetails)
                {
                    detail.DispatchAdviceHeaderId = existingEntity.Id;
                    await _applicationDbContext.DispatchAdviceDetail.AddAsync(detail);
                }
            }

            _applicationDbContext.DispatchAdviceHeader.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

    }
}
