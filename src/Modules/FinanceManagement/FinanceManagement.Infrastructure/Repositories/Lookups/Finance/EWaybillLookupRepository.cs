using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class EWaybillLookupRepository : IEWaybillLookup
    {
        private readonly IDbConnection _dbConnection;

        public EWaybillLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<EWaybillLookupDto?> GetByInvoiceAsync(string invoiceNo, int unitId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT ew.EWBNumber, ew.GeneratedDate
                FROM Finance.EWaybillHeader ew
                INNER JOIN Finance.EInvoiceHeader ei ON ew.EInvoiceHeaderId = ei.Id
                WHERE ei.InvoiceNo = @InvoiceNo AND ei.UnitId = @UnitId
                    AND ei.IsDeleted = 0 AND ew.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<EWaybillLookupDto>(
                new CommandDefinition(sql, new { InvoiceNo = invoiceNo, UnitId = unitId }, cancellationToken: ct));
        }

        public async Task<EWaybillLookupDto?> GetByDCAsync(string deliveryNumber, int unitId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1 ew.EWBNumber, ew.GeneratedDate
                FROM Finance.EWaybillHeader ew
                WHERE ew.InvoiceNo = @DeliveryNumber
                  AND ew.UnitId    = @UnitId
                  AND ew.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<EWaybillLookupDto>(
                new CommandDefinition(sql, new { DeliveryNumber = deliveryNumber, UnitId = unitId }, cancellationToken: ct));
        }
    }
}
