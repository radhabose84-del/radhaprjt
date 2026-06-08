using PurchaseManagement.Domain.Entities.Arrival;

namespace PurchaseManagement.Application.Common.Interfaces.IArrival
{
    public interface IArrivalCommandRepository
    {
        /// <summary>
        /// Inserts the arrival header + detail lines and generates one StockLedgerRaw row per bale
        /// across each line's [BaleNumberFrom..BaleNumberTo] range (DocType = ARV). Increments the
        /// document sequence for <paramref name="transactionTypeId"/> in the same transaction.
        /// </summary>
        Task<int> CreateAsync(ArrivalHeader entity, int transactionTypeId, CancellationToken ct);
        Task<int> UpdateAsync(ArrivalHeader entity, CancellationToken ct);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
