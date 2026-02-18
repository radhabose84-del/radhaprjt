#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IIssue;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.MRS;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.Issue
{
    public class IssueEntryCommandRepository : IIssueEntryCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;

        public IssueEntryCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateIssueAsync(IssueHeader issueHeader, Func<Task> publishEvents = null)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

                    return await strategy.ExecuteAsync(async () =>
                    {
                        await using var transaction =
                            await _applicationDbContext.Database.BeginTransactionAsync();

                        // 1️⃣ Insert Issue Header
                        await _applicationDbContext.IssueHeader.AddAsync(issueHeader);

                        // 2️⃣ Insert Issue Details
                        await _applicationDbContext.IssueDetail
                            .AddRangeAsync(issueHeader.IssueHeaderName);

                        // 3️⃣ Save once → generates IssueHeader.Id
                        await _applicationDbContext.SaveChangesAsync();

                        await transaction.CommitAsync();

                        // 4️⃣ Publish domain events AFTER commit
                        if (publishEvents != null)
                            await publishEvents();

                        return issueHeader.Id;
                    });
        }

        public async Task<int> CreateIssueWithLedgersAsync(IssueHeader issueHeader, List<StockLedger> stockLedgerEntries, List<SubStoreStockLedger> subStoreLedgerEntries, Func<Task> publishEvents)
        {
            var strategy = _applicationDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _applicationDbContext.Database.BeginTransactionAsync();

                 await _applicationDbContext.IssueHeader.AddAsync(issueHeader);
                 await _applicationDbContext.IssueDetail.AddRangeAsync(issueHeader.IssueHeaderName);
                 await _applicationDbContext.SaveChangesAsync(); // now Id is generated
                  // 2️⃣ Update DocNo and linkages now that we know Id
                int issueId = issueHeader.Id;

                if (stockLedgerEntries?.Any() == true)
                {
                    foreach (var entry in stockLedgerEntries)
                    {
                        entry.DocNo = issueId;
                    }
                    await _applicationDbContext.StockLedger.AddRangeAsync(stockLedgerEntries);
                }

                if (subStoreLedgerEntries?.Any() == true)
                {
                    foreach (var entry in subStoreLedgerEntries)
                    {
                        entry.DocNo = issueId;
                    }
                    await _applicationDbContext.SubStoreStockLedger.AddRangeAsync(subStoreLedgerEntries);
                }

                await _applicationDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                if (publishEvents != null)
                    await publishEvents();

                return issueHeader.Id;
            });
        }

        public async Task<string> GenerateNextCodeAsync(CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitCode = unitId > 0 ? unitId.ToString() : "NA";
            var prefix = $"ISS-{unitCode}-";

            var recent = await _applicationDbContext.IssueHeader.AsNoTracking()
                .Where(r => r.IssueNo.StartsWith(prefix))
                .OrderByDescending(r => r.Id)
                .Select(r => r.IssueNo)
                .Take(100)
                .ToListAsync(ct);

            var max = 0;
            foreach (var code in recent)
            {
                var suffix = code.Substring(prefix.Length);
                if (int.TryParse(suffix, out var n) && n > max) max = n;
            }

            return $"{prefix}{(max + 1):D2}";
        }
   
    }
}