namespace SalesManagement.Application.Common.Interfaces.ISalesReturn
{
    public interface ISalesReturnCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesReturnHeader entity, int typeId);
        Task<int> UpdateAsync(Domain.Entities.SalesReturnHeader entity, List<Domain.Entities.SalesReturnDetail> details);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task InsertStockLedgerEntriesAsync(List<Domain.Entities.StockLedger> entries);
        Task UpdateComplaintResolutionReturnStatusAsync(int complaintHeaderId, int returnStatusId, decimal returnQuantity);
    }
}
