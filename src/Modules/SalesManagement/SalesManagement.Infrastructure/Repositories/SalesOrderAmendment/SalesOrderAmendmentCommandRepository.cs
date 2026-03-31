using Microsoft.EntityFrameworkCore;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
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

        public async Task<int> CreateAsync(SalesOrderAmendmentHeader entity, List<SalesOrderAmendmentDetail> details)
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

        public async Task<bool> ApplyAmendmentAsync(int amendmentHeaderId, string status, CancellationToken ct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var amendmentHeader = await _dbContext.SalesOrderAmendmentHeader
                        .Include(h => h.SalesOrderAmendmentDetails)
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
                    amendmentHeader.ApprovedDate = DateTimeOffset.UtcNow;

                    _dbContext.SalesOrderAmendmentHeader.Update(amendmentHeader);

                    if (isApproved && amendmentHeader.SalesOrderAmendmentDetails != null)
                    {
                        var soHeader = await _dbContext.SalesOrderHeader
                            .Include(h => h.SalesOrderDetails)
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

                                    _dbContext.SalesOrderDetail.Update(soDetail);
                                }
                                else if (detail.ChangeType == "Removed")
                                {
                                    _dbContext.SalesOrderDetail.Remove(soDetail);
                                }
                            }

                            // Increment RevisionNumber
                            soHeader.RevisionNumber = amendmentHeader.RevisionNumber;
                            _dbContext.SalesOrderHeader.Update(soHeader);
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
    }
}
