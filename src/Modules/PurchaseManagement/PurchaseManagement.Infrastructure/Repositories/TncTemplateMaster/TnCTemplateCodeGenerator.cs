#nullable disable
using System.Data;
using System.Text.RegularExpressions;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster
{
    public class TnCTemplateCodeGenerator : ITnCTemplateCodeGenerator
    {

         private readonly ApplicationDbContext _dbContext;
         
         public TnCTemplateCodeGenerator(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        public async Task<string> GenerateAsync(int templateTypeId, string templateName, CancellationToken ct = default)
        {
            // 1) Prefix from MiscMaster (28 = Purchase -> "PUR", 29 = Sales -> "SAL")
            var prefix = await _dbContext.MiscMaster
                .Where(x => x.Id == templateTypeId &&
                            x.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(prefix))
                prefix = "TNC";

            // 2) Slug from template name
            var slug = Slugify(templateName);
            if (string.IsNullOrWhiteSpace(slug)) slug = "TEMPLATE";

            var baseCode = $"{prefix}-{slug}"; // e.g., "PUR-IMPORT-VENDOR"

            // 3) Pull existing codes with this base prefix into memory, then parse suffixes
            var existingCodes = await _dbContext.TnCTemplateMaster
                .Where(t =>
                    t.IsDeleted == PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted &&
                    t.TemplateCode.StartsWith(baseCode + "-"))
                .Select(t => t.TemplateCode)
                .ToListAsync(ct);

            var maxSuffix = existingCodes
                .Select(code =>
                {
                    var part = code.Substring(baseCode.Length + 1); // after "PUR-IMPORT-VENDOR-"
                    return int.TryParse(part, out var n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            var nextSuffix = maxSuffix + 1;
            var finalCode = $"{baseCode}-{nextSuffix}";

            // (Optional) ensure column length (e.g., nvarchar(50))
            const int maxLen = 50;
            if (finalCode.Length > maxLen)
                finalCode = finalCode.Substring(0, maxLen);

            return finalCode;
        }

        // Simple slugifier: keep letters/numbers, collapse spaces to '-', uppercase
        private static string Slugify(string input)
        {
            var s = input?.Trim() ?? string.Empty;
            s = s.Replace("&", "and");           
            s = Regex.Replace(s, @"[^\w\s-]", ""); // drop punctuation
            s = Regex.Replace(s, @"\s+", "-");     // spaces -> hyphen
            s = Regex.Replace(s, @"-+", "-");      // collapse multiple '-'
            return s.Trim('-').ToUpperInvariant();
        }


        
    }
}