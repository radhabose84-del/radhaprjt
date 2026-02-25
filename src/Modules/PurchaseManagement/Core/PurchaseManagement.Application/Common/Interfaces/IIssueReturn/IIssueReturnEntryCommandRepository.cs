using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.Application.Common.Interfaces.IIssueReturn
{
    public interface IIssueReturnEntryCommandRepository
    {
        Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
        Task<IssueReturnHeader> CreateAsync(IssueReturnHeader issueReturnHeader);
        Task<bool> UpdateAsync(IssueReturnHeader issueReturnHeader);
        Task<bool> FinalizeStatus(IssueReturnHeader issueReturnHeader);
        Task<int> InsertAsync(StockLedger log, CancellationToken cancellationToken = default);
        Task InsertStockAsync(StockLedger stockLedger, SubStoreStockLedger subStoreStockLedger);


        
    }
}