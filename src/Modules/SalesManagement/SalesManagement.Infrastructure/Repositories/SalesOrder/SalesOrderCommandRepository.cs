using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesOrder
{
    public class SalesOrderCommandRepository : ISalesOrderCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public SalesOrderCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<string> GenerateNextSalesOrderNoAsync(int unitId, CancellationToken ct = default)
        {
            var prefix = $"SO-{unitId}-";

            var lastOrder = await _applicationDbContext.SalesOrderHeader
                .Where(x => x.SalesOrderNo != null && x.SalesOrderNo.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.SalesOrderNo)
                .FirstOrDefaultAsync(ct);

            int nextSeq = 1;
            if (lastOrder != null)
            {
                var parts = lastOrder.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                {
                    nextSeq = lastSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }

        public async Task<int> CreateAsync(SalesOrderHeader entity)
        {
            // Separate details from header
            var details = entity.SalesOrderDetails?.ToList();
            entity.SalesOrderDetails = null;

            // Insert header into SalesOrderHeader table
            await _applicationDbContext.SalesOrderHeader.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();

            // Insert details into SalesOrderDetail table
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    detail.SalesOrderHeaderId = entity.Id;
                    await _applicationDbContext.SalesOrderDetail.AddAsync(detail);
                }
                await _applicationDbContext.SaveChangesAsync();
            }

            return entity.Id;
        }

        public async Task<int> UpdateAsync(SalesOrderHeader entity)
        {
            var existingEntity = await _applicationDbContext.SalesOrderHeader
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Update header fields in SalesOrderHeader table
            existingEntity.SalesGroupId = entity.SalesGroupId;
            existingEntity.SalesSegmentId = entity.SalesSegmentId;
            existingEntity.EnquiryType = entity.EnquiryType;
            existingEntity.UnitId = entity.UnitId;
            existingEntity.PartyId = entity.PartyId;
            existingEntity.DiscountPlanId = entity.DiscountPlanId;
            existingEntity.PaymentTermsId = entity.PaymentTermsId;
            existingEntity.PaymentTypeId = entity.PaymentTypeId;
            existingEntity.FreightTypeId = entity.FreightTypeId;
            existingEntity.CountListId = entity.CountListId;
            existingEntity.Remarks = entity.Remarks;
            existingEntity.VisitNotesAttachment = entity.VisitNotesAttachment;
            existingEntity.AgentPOAttachment = entity.AgentPOAttachment;
            existingEntity.DispatchLocationType = entity.DispatchLocationType;
            existingEntity.DispatchDepotId = entity.DispatchDepotId;
            existingEntity.DispatchUnitId = entity.DispatchUnitId;
            existingEntity.TotalBags = entity.TotalBags;
            existingEntity.TotalWeightKgs = entity.TotalWeightKgs;
            existingEntity.TotalDiscountPerKg = entity.TotalDiscountPerKg;
            existingEntity.ItemValue = entity.ItemValue;
            existingEntity.TotalFreight = entity.TotalFreight;
            existingEntity.TaxableAmount = entity.TaxableAmount;
            existingEntity.GSTPercentage = entity.GSTPercentage;
            existingEntity.TotalGST = entity.TotalGST;
            existingEntity.TotalWithGST = entity.TotalWithGST;
            existingEntity.TCSPercentage = entity.TCSPercentage;
            existingEntity.TotalTCS = entity.TotalTCS;
            existingEntity.FinalAmount = entity.FinalAmount;
            existingEntity.IsActive = entity.IsActive;

            // Remove existing details from SalesOrderDetail table
            if (existingEntity.SalesOrderDetails != null && existingEntity.SalesOrderDetails.Any())
            {
                _applicationDbContext.SalesOrderDetail.RemoveRange(existingEntity.SalesOrderDetails);
            }

            // Insert new details into SalesOrderDetail table
            if (entity.SalesOrderDetails != null && entity.SalesOrderDetails.Any())
            {
                foreach (var detail in entity.SalesOrderDetails)
                {
                    detail.SalesOrderHeaderId = existingEntity.Id;
                    await _applicationDbContext.SalesOrderDetail.AddAsync(detail);
                }
            }

            _applicationDbContext.SalesOrderHeader.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.SalesOrderHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
