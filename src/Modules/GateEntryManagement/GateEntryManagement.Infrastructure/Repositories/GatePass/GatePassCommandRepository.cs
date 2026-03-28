using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Infrastructure.Data;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Infrastructure.Repositories.GatePass
{
    public class GatePassCommandRepository : IGatePassCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IEnumerable<IGatePassDocumentHandler> _documentHandlers;
        private readonly IIPAddressService _ipAddressService;

        public GatePassCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup,
            IEnumerable<IGatePassDocumentHandler> documentHandlers,
            IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
            _documentHandlers = documentHandlers;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(GatePassHdr entity, int transactionTypeId, int vmrOutStatusId)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _applicationDbContext.GatePassHdr.AddAsync(entity);
                    await _applicationDbContext.SaveChangesAsync();

                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();

                    // Increment DocNo
                    await _documentSequenceLookup.IncrementDocNoAsync(transactionTypeId, dbConnection, dbTransaction);

                    // Mark documents as gate-passed via Strategy pattern
                    if (entity.GatePassDetails != null && entity.GatePassDetails.Count > 0)
                    {
                        // Build handler lookup: DocTypeId → TypeName → Handler
                        var docTypeIds = entity.GatePassDetails.Select(d => d.DocTypeId).Distinct().ToList();
                        var transactionTypes = await _documentSequenceLookup.GetTransactionTypesByIdsAsync(docTypeIds);
                        var typeNameMap = transactionTypes.ToDictionary(t => t.Id, t => t.TypeName ?? string.Empty);

                        foreach (var detail in entity.GatePassDetails)
                        {
                            if (detail.DocId <= 0) continue;

                            if (typeNameMap.TryGetValue(detail.DocTypeId, out var typeName))
                            {
                                var handler = _documentHandlers.FirstOrDefault(
                                    h => string.Equals(h.DocumentType, typeName, StringComparison.OrdinalIgnoreCase));

                                if (handler != null)
                                {
                                    await handler.MarkAsGatePassedAsync(detail.DocId, dbConnection, dbTransaction);
                                }
                            }
                        }
                    }

                    // Update VMR: StatusId = OUT, set GateOutTime
                    var vmr = await _applicationDbContext.VehicleMovementRecord
                        .FirstOrDefaultAsync(v => v.Id == entity.VehicleMovementRecordId && v.IsDeleted == IsDelete.NotDeleted);
                    if (vmr != null)
                    {
                        vmr.StatusId = vmrOutStatusId;
                        vmr.GateOutTime = DateTimeOffset.UtcNow;
                        vmr.GateOutBy = _ipAddressService.GetUserName();
                        _applicationDbContext.VehicleMovementRecord.Update(vmr);
                    }

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

        public async Task<bool> SoftDeleteAsync(int id, int vmrInStatusId, CancellationToken ct)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _applicationDbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var existing = await _applicationDbContext.GatePassHdr
                        .Include(g => g.GatePassDetails)
                        .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

                    if (existing == null)
                        return false;

                    var dbConnection = _applicationDbContext.Database.GetDbConnection();
                    var dbTransaction = transaction.GetDbTransaction();

                    // Revert GEFlag = 0 on linked documents (Invoice, DC) via Strategy pattern
                    if (existing.GatePassDetails != null && existing.GatePassDetails.Count > 0)
                    {
                        var docTypeIds = existing.GatePassDetails.Select(d => d.DocTypeId).Distinct().ToList();
                        var transactionTypes = await _documentSequenceLookup.GetTransactionTypesByIdsAsync(docTypeIds, ct);
                        var typeNameMap = transactionTypes.ToDictionary(t => t.Id, t => t.TypeName ?? string.Empty);

                        foreach (var detail in existing.GatePassDetails)
                        {
                            if (detail.DocId <= 0) continue;

                            if (typeNameMap.TryGetValue(detail.DocTypeId, out var typeName))
                            {
                                var handler = _documentHandlers.FirstOrDefault(
                                    h => string.Equals(h.DocumentType, typeName, StringComparison.OrdinalIgnoreCase));

                                if (handler != null)
                                {
                                    await handler.RevertGatePassAsync(detail.DocId, dbConnection, dbTransaction);
                                }
                            }
                        }
                    }

                    // Revert VMR: StatusId = IN, clear GateOutTime
                    var vmr = await _applicationDbContext.VehicleMovementRecord
                        .FirstOrDefaultAsync(v => v.Id == existing.VehicleMovementRecordId && v.IsDeleted == IsDelete.NotDeleted, ct);
                    if (vmr != null)
                    {
                        vmr.StatusId = vmrInStatusId;
                        vmr.GateOutTime =null;
                        vmr.GateOutBy=null;
                        _applicationDbContext.VehicleMovementRecord.Update(vmr);
                    }

                    // Soft-delete the gate pass
                    existing.IsDeleted = IsDelete.Deleted;
                    _applicationDbContext.GatePassHdr.Update(existing);

                    await _applicationDbContext.SaveChangesAsync(ct);
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

    }
}
