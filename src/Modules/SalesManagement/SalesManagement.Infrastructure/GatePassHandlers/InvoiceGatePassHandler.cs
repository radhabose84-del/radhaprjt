using System.Data.Common;
using Contracts.Interfaces;
using Dapper;
using SalesManagement.Domain.Common;

namespace SalesManagement.Infrastructure.GatePassHandlers
{
    internal sealed class InvoiceGatePassHandler : IGatePassDocumentHandler
    {
        public string DocumentType => MiscEnumEntity.TransactionTypeInvoice;

        public async Task MarkAsGatePassedAsync(int documentId, DbConnection connection, DbTransaction transaction)
        {
            const string sql = "UPDATE [Sales].[InvoiceHeader] SET GEFlag = 1 WHERE Id = @Id AND IsDeleted = 0";
            await connection.ExecuteAsync(sql, new { Id = documentId }, transaction);
        }
    }
}
