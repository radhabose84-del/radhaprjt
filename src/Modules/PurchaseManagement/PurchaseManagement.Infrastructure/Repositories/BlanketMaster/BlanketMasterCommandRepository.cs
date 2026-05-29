using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.BlanketMaster.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.BlanketMaster;

public sealed class BlanketMasterCommandRepository : IBlanketMasterCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IIPAddressService _ipAddressService;
    private readonly IDocumentSequenceLookup _documentSequenceLookup;

    public BlanketMasterCommandRepository(
        ApplicationDbContext db,
        IIPAddressService ipAddressService,
        IDocumentSequenceLookup documentSequenceLookup)
    {
        _db = db;
        _ipAddressService = ipAddressService;
        _documentSequenceLookup = documentSequenceLookup;
    }

    public async Task<BlanketHeader> CreateAsync(BlanketHeader entity, int transactionTypeId, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, ct);

            try
            {
                // Generate BlanketNumber
                var sequences = await _documentSequenceLookup.GenerateDocumentNumber(transactionTypeId);
                entity.BlanketNumber = sequences.Count > 0
                    ? sequences[^1]
                    : throw new ExceptionRules("No document sequence configured for Blanket Master.");

                _db.Set<BlanketHeader>().Add(entity);
                await _db.SaveChangesAsync(ct);

                var dbConnection = _db.Database.GetDbConnection();
                var dbTransaction = tx.GetDbTransaction();
                await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return entity;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    public async Task<BlanketHeader> UpdateAsync(
        BlanketHeader entity, List<BlanketDetail> details, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, ct);

            var existing = await _db.Set<BlanketHeader>()
                .Include(h => h.Details)
                    .ThenInclude(d => d.Schedules)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                ?? throw new ExceptionRules("Blanket Master not found.");

            // Update header fields
            existing.VendorId = entity.VendorId;
            existing.CurrencyId = entity.CurrencyId;
            existing.ProcurementTypeId = entity.ProcurementTypeId;
            existing.BrokerName = entity.BrokerName;
            existing.ValidityFrom = entity.ValidityFrom;
            existing.ValidityTo = entity.ValidityTo;
            existing.PaymentTerms = entity.PaymentTerms;
            existing.DeliveryTerms = entity.DeliveryTerms;
            existing.StatusId = entity.StatusId;
            existing.Remarks = entity.Remarks;
            existing.IsActive = entity.IsActive;

            // Soft-delete removed details
            var incomingDetailIds = details.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();
            var existingDetails = existing.Details.ToList();

            foreach (var existingDetail in existingDetails)
            {
                if (!incomingDetailIds.Contains(existingDetail.Id))
                {
                    existingDetail.IsDeleted = IsDelete.Deleted;
                    existingDetail.IsActive = Status.Inactive;
                    // Soft-delete child schedules
                    foreach (var sched in existingDetail.Schedules)
                    {
                        sched.IsDeleted = IsDelete.Deleted;
                        sched.IsActive = Status.Inactive;
                    }
                }
            }

            foreach (var detail in details)
            {
                if (detail.Id > 0)
                {
                    // Update existing detail
                    var existingDetail = existingDetails.FirstOrDefault(d => d.Id == detail.Id);
                    if (existingDetail != null)
                    {
                        existingDetail.ItemSno = detail.ItemSno;
                        existingDetail.ItemId = detail.ItemId;
                        existingDetail.UOMId = detail.UOMId;
                        existingDetail.EstimatedQuantity = detail.EstimatedQuantity;
                        existingDetail.Rate = detail.Rate;
                        existingDetail.TotalPrice = detail.EstimatedQuantity * detail.Rate;
                        existingDetail.HSNId = detail.HSNId;
                        existingDetail.GSTPercentage = detail.GSTPercentage;
                        existingDetail.QualitySpecification = detail.QualitySpecification;

                        // Handle schedules: soft-delete removed, update existing, add new
                        var incomingSchedIds = detail.Schedules
                            .Where(s => s.Id > 0).Select(s => s.Id).ToHashSet();

                        foreach (var existingSched in existingDetail.Schedules.ToList())
                        {
                            if (!incomingSchedIds.Contains(existingSched.Id))
                            {
                                existingSched.IsDeleted = IsDelete.Deleted;
                                existingSched.IsActive = Status.Inactive;
                            }
                        }

                        foreach (var sched in detail.Schedules)
                        {
                            if (sched.Id > 0)
                            {
                                var existingSched = existingDetail.Schedules
                                    .FirstOrDefault(s => s.Id == sched.Id);
                                if (existingSched != null)
                                {
                                    existingSched.ScheduleNo = sched.ScheduleNo;
                                    existingSched.ScheduleDate = sched.ScheduleDate;
                                    existingSched.ScheduleQuantity = sched.ScheduleQuantity;
                                    existingSched.Remarks = sched.Remarks;
                                }
                            }
                            else
                            {
                                existingDetail.Schedules.Add(new BlanketSchedule
                                {
                                    BlanketDetailId = existingDetail.Id,
                                    ScheduleNo = sched.ScheduleNo,
                                    ScheduleDate = sched.ScheduleDate,
                                    ScheduleQuantity = sched.ScheduleQuantity,
                                    Remarks = sched.Remarks,
                                    IsActive = Status.Active,
                                    IsDeleted = IsDelete.NotDeleted
                                });
                            }
                        }
                    }
                }
                else
                {
                    // New detail
                    var newDetail = new BlanketDetail
                    {
                        BlanketHeaderId = existing.Id,
                        ItemSno = detail.ItemSno,
                        ItemId = detail.ItemId,
                        UOMId = detail.UOMId,
                        EstimatedQuantity = detail.EstimatedQuantity,
                        Rate = detail.Rate,
                        TotalPrice = detail.EstimatedQuantity * detail.Rate,
                        HSNId = detail.HSNId,
                        GSTPercentage = detail.GSTPercentage,
                        QualitySpecification = detail.QualitySpecification,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    };

                    foreach (var sched in detail.Schedules)
                    {
                        newDetail.Schedules.Add(new BlanketSchedule
                        {
                            ScheduleNo = sched.ScheduleNo,
                            ScheduleDate = sched.ScheduleDate,
                            ScheduleQuantity = sched.ScheduleQuantity,
                            Remarks = sched.Remarks,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        });
                    }

                    existing.Details.Add(newDetail);
                }
            }

            // Recalculate total estimated value
            var activeDetails = existing.Details
                .Where(d => d.IsDeleted == IsDelete.NotDeleted).ToList();
            existing.TotalEstimatedValue = activeDetails.Sum(d => d.TotalPrice);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return existing;
        });
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var existing = await _db.Set<BlanketHeader>()
            .Include(h => h.Details)
                .ThenInclude(d => d.Schedules)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            return false;

        existing.IsDeleted = IsDelete.Deleted;
        existing.IsActive = Status.Inactive;

        foreach (var detail in existing.Details)
        {
            detail.IsDeleted = IsDelete.Deleted;
            detail.IsActive = Status.Inactive;
            foreach (var sched in detail.Schedules)
            {
                sched.IsDeleted = IsDelete.Deleted;
                sched.IsActive = Status.Inactive;
            }
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateBlanketApproveAsync(int id, int statusId, CancellationToken ct)
    {
        var existing = await _db.Set<BlanketHeader>()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

        if (existing is null)
            return false;

        existing.StatusId = statusId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BlanketMasterWorkFlowDto> GetByIdBlanketWorkFlowAsync(int id)
    {
        var entity = await _db.Set<BlanketHeader>()
            .Where(x => x.Id == id)
            .Select(x => new BlanketMasterWorkFlowDto
            {
                Id = x.Id,
                BlanketNumber = x.BlanketNumber,
                VendorId = x.VendorId,
                StatusId = x.StatusId,
                UnitId = x.UnitId
            })
            .FirstOrDefaultAsync();

        return entity!;
    }
}
