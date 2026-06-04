#nullable disable
using System.Data;
using Contracts.Interfaces.Lookups.Finance;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster
{
    public class TnCTemplateCodeGenerator : ITnCTemplateCodeGenerator
    {

         private readonly ApplicationDbContext _dbContext;
         private readonly ITransactionTypeLookup _transactionTypeLookup;

         public TnCTemplateCodeGenerator(ApplicationDbContext context, ITransactionTypeLookup transactionTypeLookup)
        {
            _dbContext = context;
            _transactionTypeLookup = transactionTypeLookup;
        }

        // Produces "{ShortName}-{00001}" e.g. "PO-00001", running number per prefix.
        public async Task<string> GenerateAsync(int transactionTypeId, CancellationToken ct = default)
        {
            // 1) Prefix from the TransactionType ShortName (cross-module lookup, no JOIN)
            var prefix = "TNC";
            if (transactionTypeId > 0)
            {
                var types = await _transactionTypeLookup.GetByIdsAsync(new[] { transactionTypeId });
                var shortName = types.FirstOrDefault()?.ShortName;
                if (!string.IsNullOrWhiteSpace(shortName))
                    prefix = shortName.Trim().ToUpperInvariant();
            }

            // 2) Find the highest existing running number for this prefix (across all rows,
            //    deleted included, so numbers are never reused).
            var existingCodes = await _dbContext.TnCTemplateMaster
                .Where(t => t.TemplateCode.StartsWith(prefix + "-"))
                .Select(t => t.TemplateCode)
                .ToListAsync(ct);

            var maxSeq = existingCodes
                .Select(code =>
                {
                    var part = code.Substring(prefix.Length + 1); // after "PO-"
                    return int.TryParse(part, out var n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var nextSeq = maxSeq + 1;
            return $"{prefix}-{nextSeq:D5}";
        }
    }
}
