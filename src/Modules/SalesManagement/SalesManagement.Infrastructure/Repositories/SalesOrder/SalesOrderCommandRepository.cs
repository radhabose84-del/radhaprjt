using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;

        public SalesOrderCommandRepository(
            ApplicationDbContext applicationDbContext,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(SalesOrderHeader entity, int transactionTypeId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    // Separate details + discounts from header
                    var details = entity.SalesOrderDetails?.ToList();
                    var discounts = entity.SalesOrderDiscounts?.ToList();
                    entity.SalesOrderDetails = null;
                    entity.SalesOrderDiscounts = null;

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

                    // Insert discounts into SalesOrderDiscount table
                    if (discounts != null && discounts.Count > 0)
                    {
                        foreach (var discount in discounts)
                        {
                            discount.SalesOrderHeaderId = entity.Id;
                            await _applicationDbContext.SalesOrderDiscount.AddAsync(discount);
                        }
                        await _applicationDbContext.SaveChangesAsync();
                    }

                    // Increment DocNo via lookup — same connection + transaction
                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

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

        public async Task<int> UpdateAsync(SalesOrderHeader entity)
        {
            var existingEntity = await _applicationDbContext.SalesOrderHeader
                .Include(h => h.SalesOrderDetails)
                .Include(h => h.SalesOrderDiscounts)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existingEntity == null)
                return 0;

            // Update header fields in SalesOrderHeader table
            existingEntity.SalesGroupId = entity.SalesGroupId;
            existingEntity.SalesSegmentId = entity.SalesSegmentId;
            existingEntity.EnquiryType = entity.EnquiryType;
            existingEntity.UnitId = entity.UnitId;
            existingEntity.PartyId = entity.PartyId;
            existingEntity.PaymentTypeId = entity.PaymentTypeId;
            existingEntity.AgentCommissionId = entity.AgentCommissionId;
            existingEntity.AgentPaymentTermsId = entity.AgentPaymentTermsId;
            existingEntity.AgentCommissionSlabId = entity.AgentCommissionSlabId;
            existingEntity.CommissionRate = entity.CommissionRate;
            existingEntity.CommissionValue = entity.CommissionValue;
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
            existingEntity.SalesOrderTypeId = entity.SalesOrderTypeId;
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

            // Replace existing discounts with new set
            if (existingEntity.SalesOrderDiscounts != null && existingEntity.SalesOrderDiscounts.Any())
            {
                _applicationDbContext.SalesOrderDiscount.RemoveRange(existingEntity.SalesOrderDiscounts);
            }

            if (entity.SalesOrderDiscounts != null && entity.SalesOrderDiscounts.Any())
            {
                foreach (var discount in entity.SalesOrderDiscounts)
                {
                    discount.Id = 0;   // force insert (in case Update DTO carried Ids)
                    discount.SalesOrderHeaderId = existingEntity.Id;
                    await _applicationDbContext.SalesOrderDiscount.AddAsync(discount);
                }
            }

            _applicationDbContext.SalesOrderHeader.Update(existingEntity);
            await _applicationDbContext.SaveChangesAsync();
            return existingEntity.Id;
        }

        public async Task<bool> CancelAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var cancelledStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusCancelled);
            existing.StatusId = cancelledStatus?.Id;

            existing.CancelledDate = DateTimeOffset.UtcNow;
            existing.CancelledByName = _ipAddressService.GetUserName();
            existing.CancelledIP = _ipAddressService.GetUserIPAddress();

            _applicationDbContext.SalesOrderHeader.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ForecloseAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            var foreclosedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusForeClosed);
            existing.StatusId = foreclosedStatus?.Id;

            existing.ForeClosedDate = DateTimeOffset.UtcNow;
            existing.ForeClosedByName = _ipAddressService.GetUserName();
            existing.ForeClosedIP = _ipAddressService.GetUserIPAddress();

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

        public async Task<bool> UpdateMdApprovalDocumentAsync(int id, string fileName, CancellationToken ct)
        {
            var existing = await _applicationDbContext.SalesOrderHeader
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.MdApprovalDocument = fileName;
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
                    UnitId = x.OrderUnitId
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
