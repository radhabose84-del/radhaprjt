using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesOrderAmendment
{
    public class SalesOrderAmendmentCommandRepository : ISalesOrderAmendmentCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public SalesOrderAmendmentCommandRepository(
            ApplicationDbContext dbContext,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(SalesOrderAmendmentHeader entity, List<SalesOrderAmendmentDetail> details, List<SalesOrderAmendmentDiscount> discounts)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // Set StatusId to Pending
                    var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                        MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusPending);
                    entity.StatusId = pendingStatus?.Id;

                    await _dbContext.SalesOrderAmendmentHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    foreach (var detail in details)
                    {
                        detail.SalesOrderAmendmentHeaderId = entity.Id;
                        await _dbContext.SalesOrderAmendmentDetail.AddAsync(detail);
                    }

                    foreach (var discount in discounts)
                    {
                        discount.SalesOrderAmendmentHeaderId = entity.Id;
                        await _dbContext.SalesOrderAmendmentDiscount.AddAsync(discount);
                    }
                    await _dbContext.SaveChangesAsync();

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

        public async Task<bool> ApplyAmendmentAsync(int amendmentHeaderId, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var amendmentHeader = await _dbContext.SalesOrderAmendmentHeader
                        .Include(h => h.SalesOrderAmendmentDetails)
                        .Include(h => h.SalesOrderAmendmentDiscounts)
                        .FirstOrDefaultAsync(h => h.Id == amendmentHeaderId && h.IsDeleted == IsDelete.NotDeleted, ct);

                    if (amendmentHeader == null)
                        return false;

                    // Resolve status IDs
                    var approvedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                        MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusApproved);
                    var rejectedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                        MiscEnumEntity.SalesOrderApprovalStatus, MiscEnumEntity.SalesOrderStatusRejected);

                    var isApproved = status == MiscEnumEntity.SalesOrderStatusApproved;

                    amendmentHeader.StatusId = isApproved
                        ? approvedStatus?.Id
                        : rejectedStatus?.Id;
                    amendmentHeader.ApprovedBy = modifiedBy;
                    amendmentHeader.ApprovedDate = DateTimeOffset.UtcNow;
                    amendmentHeader.ModifiedBy = modifiedBy;
                    amendmentHeader.ModifiedByName = modifiedByName;
                    amendmentHeader.ModifiedIP = modifiedIP;
                    amendmentHeader.ModifiedDate = DateTimeOffset.UtcNow;

                    _dbContext.SalesOrderAmendmentHeader.Update(amendmentHeader);

                    if (isApproved && amendmentHeader.SalesOrderAmendmentDetails != null)
                    {
                        var soHeader = await _dbContext.SalesOrderHeader
                            .Include(h => h.SalesOrderDetails)
                            .Include(h => h.SalesOrderDiscounts)
                            .FirstOrDefaultAsync(h => h.Id == amendmentHeader.SalesOrderHeaderId, ct);

                        if (soHeader != null)
                        {
                            foreach (var detail in amendmentHeader.SalesOrderAmendmentDetails)
                            {
                                var soDetail = soHeader.SalesOrderDetails?
                                    .FirstOrDefault(d => d.Id == detail.SalesOrderDetailId);

                                if (soDetail == null) continue;

                                if (detail.ChangeType == "Modified")
                                {
                                    if (detail.NewQtyInBags.HasValue)
                                    {
                                        soDetail.PendingQty += detail.NewQtyInBags.Value - soDetail.QtyInBags;
                                        soDetail.QtyInBags = detail.NewQtyInBags.Value;
                                    }
                                    if (detail.NewExMillRate.HasValue)
                                        soDetail.ExMillRate = detail.NewExMillRate.Value;
                                    if (detail.NewExpectedDeliveryDate.HasValue)
                                        soDetail.ExpectedDeliveryDate = detail.NewExpectedDeliveryDate.Value;

                                    // Update detail-level computed fields
                                    soDetail.TotalWeight = detail.TotalWeight;
                                    soDetail.DiscountPerUnit = detail.DiscountPerUnit;
                                    soDetail.TaxableAmount = detail.TaxableAmount;
                                    soDetail.TaxAmount = detail.TaxAmount;
                                    soDetail.TCSAmount = detail.TCSAmount;
                                    soDetail.NetAmount = detail.NetAmount;
                                    soDetail.NetRatePerKg = detail.NetRatePerKg;
                                    soDetail.PendingQty = detail.PendingQty;

                                    // Propagate per-line agent commission %
                                    if (detail.AgentCommissionPercentage.HasValue)
                                        soDetail.AgentCommissionPercentage = detail.AgentCommissionPercentage.Value;

                                    _dbContext.SalesOrderDetail.Update(soDetail);
                                }
                                else if (detail.ChangeType == "Removed")
                                {
                                    // Soft delete — physical delete blocked by AmendmentDetail FK
                                    soDetail.PendingQty = 0;
                                    soDetail.QtyInBags = 0;
                                    var closedStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                                        MiscEnumEntity.LineItemApprovalStatus, MiscEnumEntity.LineStatusDeleted);
                                    soDetail.LineItemStatusId = closedStatus?.Id;
                                    _dbContext.SalesOrderDetail.Update(soDetail);
                                }
                            }

                            // Update header-level summary fields from amendment
                            soHeader.TotalBags = amendmentHeader.TotalBags;
                            soHeader.TotalWeightKgs = amendmentHeader.TotalWeightKgs;
                            soHeader.TotalDiscountPerKg = amendmentHeader.TotalDiscountPerKg;
                            soHeader.ItemValue = amendmentHeader.ItemValue;
                            soHeader.TotalFreight = amendmentHeader.TotalFreight;
                            soHeader.TaxableAmount = amendmentHeader.TaxableAmount;
                            soHeader.GSTPercentage = amendmentHeader.GSTPercentage;
                            soHeader.TotalGST = amendmentHeader.TotalGST;
                            soHeader.TotalWithGST = amendmentHeader.TotalWithGST;
                            soHeader.TCSPercentage = amendmentHeader.TCSPercentage;
                            soHeader.TotalTCS = amendmentHeader.TotalTCS;
                            soHeader.FinalAmount = amendmentHeader.FinalAmount;

                            // Propagate Agent Commission + Discount snapshot.
                            // AgentCommissionId and AgentCommissionSlabId are FK-constrained on SalesOrderHeader
                            // (FK_SalesOrderHeader_AgentCommissionConfig_AgentCommissionId / _AgentCommissionSlab_…).
                            // FE / amendment payloads sometimes send 0 to mean "not selected"; the amendment header
                            // tolerates this (no FK there) but propagating 0 to SalesOrderHeader fails with SQL 547.
                            // Coerce <=0 → null so "not selected" survives the FK check. AgentPaymentTermsId has no
                            // DB FK and the column is NOT NULL, so 0 passes through unchanged.
                            soHeader.AgentCommissionId     = amendmentHeader.AgentCommissionId     is > 0 ? amendmentHeader.AgentCommissionId     : null;
                            soHeader.AgentCommissionSlabId = amendmentHeader.AgentCommissionSlabId is > 0 ? amendmentHeader.AgentCommissionSlabId : null;
                            soHeader.AgentPaymentTermsId   = amendmentHeader.AgentPaymentTermsId;
                            soHeader.CommissionRate = amendmentHeader.CommissionRate;
                            soHeader.CommissionValue = amendmentHeader.CommissionValue;
                            soHeader.MdDiscountValue = amendmentHeader.MdDiscountValue;
                            soHeader.TotalDiscountValue = amendmentHeader.TotalDiscountValue;

                            // Increment RevisionNumber
                            soHeader.RevisionNumber = amendmentHeader.RevisionNumber;
                            _dbContext.SalesOrderHeader.Update(soHeader);

                            // Replace SalesOrderDiscount rows from snapshot
                            if (amendmentHeader.SalesOrderAmendmentDiscounts != null)
                            {
                                if (soHeader.SalesOrderDiscounts != null && soHeader.SalesOrderDiscounts.Count > 0)
                                {
                                    _dbContext.SalesOrderDiscount.RemoveRange(soHeader.SalesOrderDiscounts);
                                }

                                foreach (var ad in amendmentHeader.SalesOrderAmendmentDiscounts)
                                {
                                    await _dbContext.SalesOrderDiscount.AddAsync(new SalesOrderDiscount
                                    {
                                        SalesOrderHeaderId = soHeader.Id,
                                        DiscountMasterId = ad.DiscountMasterId,
                                        SlabTypeId = ad.SlabTypeId,
                                        PaymentTermId = ad.PaymentTermId,
                                        DiscountSlabId = ad.DiscountSlabId,
                                        DiscountRate = ad.DiscountRate,
                                        TotalDiscountValue = ad.TotalDiscountValue
                                    }, ct);
                                }
                            }
                        }
                    }

                    await _dbContext.SaveChangesAsync(ct);
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

        public async Task<SalesOrderAmendmentHeader?> GetByIdEntityAsync(int id)
        {
            return await _dbContext.SalesOrderAmendmentHeader
                .Include(h => h.SalesOrderAmendmentDetails)
                .FirstOrDefaultAsync(h => h.Id == id && h.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task<SalesOrderHeader?> GetSalesOrderEntityAsync(int salesOrderHeaderId)
        {
            return await _dbContext.SalesOrderHeader
                .Include(h => h.SalesOrderDetails)
                .FirstOrDefaultAsync(h => h.Id == salesOrderHeaderId && h.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task<AmendmentWorkFlowDto> GetByIdAmendmentWorkFlowAsync(int id)
        {
            var entity = await _dbContext.SalesOrderAmendmentHeader
                .Where(x => x.Id == id)
                .Select(x => new AmendmentWorkFlowDto
                {
                    Id = x.Id,
                    AmendmentNo = x.AmendmentNo,
                    SalesOrderHeaderId = x.SalesOrderHeaderId,
                    SalesOrderNo = x.SalesOrderHeader != null ? x.SalesOrderHeader.SalesOrderNo : null,
                    StatusId = x.StatusId,
                    StatusName = x.StatusMisc != null ? x.StatusMisc.Description : null,
                    UnitId = x.UnitId
                })
                .FirstOrDefaultAsync();

            return entity!;
        }
    }
}
