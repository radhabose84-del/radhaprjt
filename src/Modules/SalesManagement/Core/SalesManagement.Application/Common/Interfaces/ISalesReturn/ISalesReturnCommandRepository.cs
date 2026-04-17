namespace SalesManagement.Application.Common.Interfaces.ISalesReturn
{
    public interface ISalesReturnCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesReturnHeader entity, int typeId);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task InsertStockLedgerEntriesAsync(List<Domain.Entities.StockLedger> entries);
        Task UpdateHeaderStatusAsync(int id, int statusId);
        Task UpdateComplaintResolutionReturnStatusAsync(int complaintHeaderId, int returnStatusId, decimal returnQuantity, int? closureStatusId = null, int? closedBy = null);
    }
}
