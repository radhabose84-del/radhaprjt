using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;
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

        public GatePassCommandRepository(
            ApplicationDbContext applicationDbContext,
            IDocumentSequenceLookup documentSequenceLookup,
            IEnumerable<IGatePassDocumentHandler> documentHandlers)
        {
            _applicationDbContext = applicationDbContext;
            _documentSequenceLookup = documentSequenceLookup;
            _documentHandlers = documentHandlers;
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
                        var typeNameMap = await ResolveDocTypeNamesAsync(docTypeIds, dbConnection, dbTransaction);

                        foreach (var detail in entity.GatePassDetails)
                        {
                            if (!int.TryParse(detail.DocNo, out var docId)) continue;

                            if (typeNameMap.TryGetValue(detail.DocTypeId, out var typeName))
                            {
                                var handler = _documentHandlers.FirstOrDefault(
                                    h => string.Equals(h.DocumentType, typeName, StringComparison.OrdinalIgnoreCase));

                                if (handler != null)
                                {
                                    await handler.MarkAsGatePassedAsync(docId, dbConnection, dbTransaction);
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

        public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.GatePassHdr
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.GatePassHdr.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        /// <summary>
        /// Resolves DocTypeId → TypeName from Finance.TransactionTypeMaster
        /// </summary>
        private static async Task<Dictionary<int, string>> ResolveDocTypeNamesAsync(
            List<int> docTypeIds, System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
        {
            const string sql = @"
                SELECT Id, TypeName
                FROM [Finance].[TransactionTypeMaster]
                WHERE Id IN @Ids AND IsDeleted = 0";

            var rows = await connection.QueryAsync<(int Id, string TypeName)>(sql, new { Ids = docTypeIds }, transaction);
            return rows.ToDictionary(r => r.Id, r => r.TypeName);
        }
    }
}
