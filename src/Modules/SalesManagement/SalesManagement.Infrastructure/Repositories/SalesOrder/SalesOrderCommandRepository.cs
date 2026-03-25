using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesOrder
{
    public class SalesOrderCommandRepository : ISalesOrderCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public SalesOrderCommandRepository(
            ApplicationDbContext applicationDbContext,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _applicationDbContext = applicationDbContext;
            _miscMasterQueryRepository = miscMasterQueryRepository;
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

            // Set default StatusId to "Pending"
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusPending);
            entity.StatusId = pendingStatus?.Id;

            // Fetch "Open" line item status id
            var openStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.LineItemApprovalStatus, MiscEnumEntity.LineStatusOpen);

            // Insert header into SalesOrderHeader table
            await _applicationDbContext.SalesOrderHeader.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();

            // Insert details into SalesOrderDetail table
            if (details != null && details.Count > 0)
            {
                foreach (var detail in details)
                {
                    detail.SalesOrderHeaderId = entity.Id;
                    detail.LineItemStatusId = openStatus?.Id;
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
                var openStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                    MiscEnumEntity.LineItemApprovalStatus, MiscEnumEntity.LineStatusOpen);

                foreach (var detail in entity.SalesOrderDetails)
                {
                    detail.SalesOrderHeaderId = existingEntity.Id;
                    detail.LineItemStatusId = openStatus?.Id;
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

        public async Task<bool> UpdateVisitNotesAttachmentAsync(int id, string fileName, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.VisitNotesAttachment = fileName;
            _applicationDbContext.SalesOrderHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateAgentPOAttachmentAsync(int id, string fileName, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.AgentPOAttachment = fileName;
            _applicationDbContext.SalesOrderHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<SalesOrderWorkFlowDto> GetByIdSalesOrderWorkFlowAsync(int id)
        {
            var entity = await _applicationDbContext.SalesOrderHeader
                .Where(x => x.Id == id)
                .Select(x => new SalesOrderWorkFlowDto
                {
                    Id = x.Id,
                    SalesOrderNo = x.SalesOrderNo,
                    StatusId = x.StatusId,
                    StatusName = x.StatusMisc != null ? x.StatusMisc.Description : null,
                    UnitId = x.UnitId
                })
                .FirstOrDefaultAsync();

            return entity!;
        }

        public async Task<SalesOrderHeader?> GetByIdEntityAsync(int id)
        {
            return await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task<bool> FinalizeOrderStatusAsync(SalesOrderHeader entity)
        {
            var existingOrder = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (existingOrder == null)
                return false;

            existingOrder.StatusId = entity.StatusId;
            _applicationDbContext.SalesOrderHeader.Update(existingOrder);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
    }
}
