using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Infrastructure.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Infrastructure.Repositories.SalesQuotationAmendment
{
    public class SalesQuotationAmendmentCommandRepository : ISalesQuotationAmendmentCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

        public SalesQuotationAmendmentCommandRepository(
            ApplicationDbContext dbContext,
            IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            _dbContext = dbContext;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }

        public async Task<int> CreateAsync(SalesQuotationAmendmentHeader entity, List<SalesQuotationAmendmentDetail> details)
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

                    await _dbContext.SalesQuotationAmendmentHeader.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();

                    foreach (var detail in details)
                    {
                        detail.SalesQuotationAmendmentHeaderId = entity.Id;
                        await _dbContext.SalesQuotationAmendmentDetail.AddAsync(detail);
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
                    var amendmentHeader = await _dbContext.SalesQuotationAmendmentHeader
                        .Include(h => h.SalesQuotationAmendmentDetails)
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

                    _dbContext.SalesQuotationAmendmentHeader.Update(amendmentHeader);

                    if (isApproved && amendmentHeader.SalesQuotationAmendmentDetails != null)
                    {
                        var sqHeader = await _dbContext.SalesQuotationHeader
                            .Include(h => h.SalesQuotationDetails)
                            .FirstOrDefaultAsync(h => h.Id == amendmentHeader.SalesQuotationHeaderId, ct);

                        if (sqHeader != null)
                        {
                            var detailsToRemove = new List<SalesQuotationDetail>();

                            foreach (var detail in amendmentHeader.SalesQuotationAmendmentDetails)
                            {
                                var sqDetail = sqHeader.SalesQuotationDetails?
                                    .FirstOrDefault(d => d.Id == detail.SalesQuotationDetailId);

                                if (sqDetail == null) continue;

                                if (detail.ChangeType == "Modified")
                                {
                                    if (detail.NewItemId.HasValue)
                                        sqDetail.ItemId = detail.NewItemId.Value;
                                    if (detail.NewQuantity.HasValue)
                                        sqDetail.Quantity = detail.NewQuantity.Value;
                                    if (detail.NewExMillRate.HasValue)
                                        sqDetail.ExMillRate = detail.NewExMillRate.Value;
                                    if (detail.NewDiscount.HasValue)
                                        sqDetail.Discount = detail.NewDiscount.Value;
                                    if (detail.NewHSNId.HasValue)
                                        sqDetail.HSNId = detail.NewHSNId.Value;
                                    if (detail.NewTaxPercentage.HasValue)
                                        sqDetail.TaxPercentage = detail.NewTaxPercentage.Value;

                                    // Update computed fields
                                    sqDetail.NetRate = detail.NetRate;
                                    sqDetail.TotalAmount = detail.TotalAmount;
                                    sqDetail.TaxAmount = detail.TaxAmount;

                                    _dbContext.SalesQuotationDetail.Update(sqDetail);
                                }
                                else if (detail.ChangeType == "Removed")
                                {
                                    // Physical delete — quotation details have no IsDeleted/LineItemStatusId
                                    detailsToRemove.Add(sqDetail);
                                }
                            }

                            // Remove "Removed" detail lines physically
                            if (detailsToRemove.Count > 0)
                            {
                                _dbContext.SalesQuotationDetail.RemoveRange(detailsToRemove);
                            }

                            // Update header-level summary fields from amendment
                            sqHeader.FreightCharges = amendmentHeader.FreightCharges;
                            sqHeader.OtherCharges = amendmentHeader.OtherCharges;
                            sqHeader.TotalBasicAmount = amendmentHeader.TotalBasicAmount;
                            sqHeader.TotalDiscount = amendmentHeader.TotalDiscount;
                            sqHeader.NetTaxableAmount = amendmentHeader.NetTaxableAmount;
                            sqHeader.TotalTax = amendmentHeader.TotalTax;
                            sqHeader.GrandTotal = amendmentHeader.GrandTotal;

                            // Increment RevisionNumber
                            sqHeader.RevisionNumber = amendmentHeader.RevisionNumber;
                            _dbContext.SalesQuotationHeader.Update(sqHeader);
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

        public async Task<SalesQuotationAmendmentHeader?> GetByIdEntityAsync(int id)
        {
            return await _dbContext.SalesQuotationAmendmentHeader
                .Include(h => h.SalesQuotationAmendmentDetails)
                .FirstOrDefaultAsync(h => h.Id == id && h.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task<SalesQuotationHeader?> GetSalesQuotationEntityAsync(int salesQuotationHeaderId)
        {
            return await _dbContext.SalesQuotationHeader
                .Include(h => h.SalesQuotationDetails)
                .FirstOrDefaultAsync(h => h.Id == salesQuotationHeaderId && h.IsDeleted == IsDelete.NotDeleted);
        }

        public async Task<AmendmentWorkFlowDto> GetByIdAmendmentWorkFlowAsync(int id)
        {
            var entity = await _dbContext.SalesQuotationAmendmentHeader
                .Where(x => x.Id == id)
                .Select(x => new AmendmentWorkFlowDto
                {
                    Id = x.Id,
                    AmendmentNo = x.AmendmentNo,
                    SalesQuotationHeaderId = x.SalesQuotationHeaderId,
                    QuotationNo = x.SalesQuotationHeader != null ? x.SalesQuotationHeader.QuotationNo : null,
                    StatusId = x.StatusId,
                    StatusName = x.StatusMisc != null ? x.StatusMisc.Description : null,
                    UnitId = x.UnitId
                })
                .FirstOrDefaultAsync();

            return entity!;
        }
    }
}
