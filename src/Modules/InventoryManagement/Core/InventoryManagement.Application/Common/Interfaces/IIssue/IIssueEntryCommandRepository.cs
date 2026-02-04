using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Domain.Entities.Issue;
using InventoryManagement.Domain.Entities.Stock;

namespace InventoryManagement.Application.Common.Interfaces.IIssue
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