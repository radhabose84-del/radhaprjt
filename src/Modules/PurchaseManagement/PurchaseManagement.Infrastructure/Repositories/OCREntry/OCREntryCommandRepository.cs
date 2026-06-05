using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Infrastructure.Data;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Infrastructure.Repositories.OCREntry
{
    public sealed class OCREntryCommandRepository : IOCREntryCommandRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;

        public OCREntryCommandRepository(
            ApplicationDbContext db,
            IIPAddressService ipAddressService,
            IDocumentSequenceLookup documentSequenceLookup)
        {
            _db = db;
            _ipAddressService = ipAddressService;
            _documentSequenceLookup = documentSequenceLookup;
        }

        public async Task<Domain.Entities.OCREntry> CreateAsync(Domain.Entities.OCREntry entity, int transactionTypeId, CancellationToken ct)
        {
            var strategy = _db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.ReadCommitted, ct);

                try
                {
                    _db.Set<Domain.Entities.OCREntry>().Add(entity);
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

        public async Task<int> UpdateAsync(Domain.Entities.OCREntry entity, CancellationToken ct)
        {
            var existing = await _db.Set<Domain.Entities.OCREntry>()
                .Include(x => x.OcrQualityParameters)
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted, ct)
                ?? throw new ExceptionRules("OCR not found.");

            // OcrNumber is immutable; StatusId is workflow-controlled — never updated here.
            existing.OcrDate = entity.OcrDate;
            existing.ProcurementSourceId = entity.ProcurementSourceId;
            existing.ProcurementTypeId = entity.ProcurementTypeId;
            existing.BrokerDirectId = entity.BrokerDirectId;
            existing.BrokerName = entity.BrokerName;
            existing.GradeId = entity.GradeId;
            existing.PaymentTermId = entity.PaymentTermId;
            existing.SupplierId = entity.SupplierId;
            existing.LocationId = entity.LocationId;
            existing.StationId = entity.StationId;
            existing.ItemId = entity.ItemId;
            existing.CountId = entity.CountId;
            existing.Quantity = entity.Quantity;
            existing.Weight = entity.Weight;
            existing.Rate = entity.Rate;
            existing.ExpectedDispatchDate = entity.ExpectedDispatchDate;
            existing.DocumentPath = entity.DocumentPath;

            // Additional Cotton Details
            existing.PaymentModeId = entity.PaymentModeId;
            existing.UomId = entity.UomId;
            existing.WeighmentId = entity.WeighmentId;
            existing.TransitInsuranceId = entity.TransitInsuranceId;
            existing.LorryFreightId = entity.LorryFreightId;
            existing.MillSampleNo = entity.MillSampleNo;
            existing.CottonPassedBy = entity.CottonPassedBy;
            existing.GstPercentage = entity.GstPercentage;
            existing.DiscountPercentage = entity.DiscountPercentage;
            existing.InsurancePercentage = entity.InsurancePercentage;
            existing.Remarks = entity.Remarks;
            existing.QualityTemplateId = entity.QualityTemplateId;

            existing.IsActive = entity.IsActive;

            // Replace-all cotton-quality parameters: drop existing rows, insert the submitted set.
            if (existing.OcrQualityParameters is { Count: > 0 })
                _db.Set<Domain.Entities.OCRQualityParameter>().RemoveRange(existing.OcrQualityParameters);

            if (entity.OcrQualityParameters is { Count: > 0 })
            {
                foreach (var child in entity.OcrQualityParameters)
                {
                    child.Id = 0;
                    child.OcrId = existing.Id;
                }
                await _db.Set<Domain.Entities.OCRQualityParameter>().AddRangeAsync(entity.OcrQualityParameters, ct);
            }

            await _db.SaveChangesAsync(ct);
            return existing.Id;
        }

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _db.Set<Domain.Entities.OCREntry>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            existing.IsActive = Status.Inactive;
            existing.ModifiedBy = _ipAddressService.GetUserId();
            existing.ModifiedByName = _ipAddressService.GetUserName();
            existing.ModifiedIP = _ipAddressService.GetUserIPAddress();
            existing.ModifiedDate = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ClearDocumentPathByFileNameAsync(string fileName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var existing = await _db.Set<Domain.Entities.OCREntry>()
                .FirstOrDefaultAsync(x => x.DocumentPath == fileName && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            existing.DocumentPath = null;
            existing.ModifiedBy = _ipAddressService.GetUserId();
            existing.ModifiedByName = _ipAddressService.GetUserName();
            existing.ModifiedIP = _ipAddressService.GetUserIPAddress();
            existing.ModifiedDate = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> UpdateOcrApproveAsync(int id, int statusId, CancellationToken ct)
        {
            var existing = await _db.Set<Domain.Entities.OCREntry>()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing is null)
                return false;

            existing.StatusId = statusId;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<OCREntryWorkFlowDto> GetByIdOCRWorkFlowAsync(int id)
        {
            var entity = await _db.Set<Domain.Entities.OCREntry>()
                .Where(x => x.Id == id)
                .Select(x => new OCREntryWorkFlowDto
                {
                    Id = x.Id,
                    OcrNumber = x.OcrNumber,
                    SupplierId = x.SupplierId,
                    StatusId = x.StatusId
                })
                .FirstOrDefaultAsync();

            return entity!;
        }
    }
}
