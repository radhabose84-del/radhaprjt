// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
// using MediatR;
// using Microsoft.Extensions.Configuration;

// namespace PurchaseManagement.Application.PurchaseOrder.Reports;

// public sealed class GenerateUntitledPdfHandler(
//     ISsrsClient ssrs, IConfiguration cfg) : IRequestHandler<GenerateUntitledPdfQuery, byte[]>
// {
//     public Task<byte[]> Handle(GenerateUntitledPdfQuery q, CancellationToken ct)
//     {
//         var path = cfg["Ssrs:PoReportPath"]!; // "/POReport"
//         var p = new Dictionary<string,string?> 
//         { 
//             ["unitid"] = q.UnitId.ToString(),
//             ["PoId"]  = q.PoId.ToString()
//         };
//         return ssrs.RenderPdfAsync(path, p, ct);

//     }
// }
