using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.MRS;

namespace PurchaseManagement.Application.Common.Interfaces.IIssue
{
    public interface IIssueEntryCommandRepository
    {
        Task<string> GenerateNextCodeAsync(CancellationToken ct = default);
        Task<int> CreateIssueWithLedgersAsync(
            IssueHeader issueHeader,
            List<StockLedger> stockLedgerEntries,
            List<SubStoreStockLedger> subStoreLedgerEntries,
            Func<Task>? publishEvents);

        Task<int> CreateIssueAsync(
            IssueHeader issueHeader,
            Func<Task>? publishEvents = null);

        
    }
}