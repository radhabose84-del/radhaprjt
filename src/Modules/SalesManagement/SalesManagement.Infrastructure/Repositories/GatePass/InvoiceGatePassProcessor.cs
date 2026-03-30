using System.Data.Common;
using Contracts.Interfaces;
using Dapper;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.Repositories.GatePass
{
    internal sealed class InvoiceGatePassProcessor : IGatePassDocumentProcessor
    {
        public string DocumentType => MiscEnumEntity.TransactionTypeInvoice;

        public async Task MarkAsGatePassedAsync(int documentId, int unitId, DbConnection connection, DbTransaction transaction)
        {
            const string sql = "UPDATE [Sales].[InvoiceHeader] SET GEFlag = 1 WHERE Id = @Id AND UnitId = @UnitId AND IsDeleted = 0";
            await connection.ExecuteAsync(sql, new { Id = documentId, UnitId = unitId }, transaction);
        }

        public async Task RevertGatePassAsync(int documentId, int unitId, DbConnection connection, DbTransaction transaction)
        {
            const string sql = "UPDATE [Sales].[InvoiceHeader] SET GEFlag = 0 WHERE Id = @Id AND UnitId = @UnitId AND IsDeleted = 0";
            await connection.ExecuteAsync(sql, new { Id = documentId, UnitId = unitId }, transaction);
        }
    }
}
