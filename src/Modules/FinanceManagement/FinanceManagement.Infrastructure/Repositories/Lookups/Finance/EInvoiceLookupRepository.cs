using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class EInvoiceLookupRepository : IEInvoiceLookup
    {
        private readonly IDbConnection _dbConnection;

        public EInvoiceLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<EInvoiceLookupDto?> GetByInvoiceAsync(string invoiceNo, int unitId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT ei.Id, ei.IrnNumber, ei.AckNo, ei.AckDate
                FROM Finance.EInvoiceHeader ei
                WHERE ei.InvoiceNo = @InvoiceNo AND ei.UnitId = @UnitId AND ei.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<EInvoiceLookupDto>(
                new CommandDefinition(sql, new { InvoiceNo = invoiceNo, UnitId = unitId }, cancellationToken: ct));
        }
    }
}
