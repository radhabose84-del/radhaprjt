namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;

public interface ISsrsClient
{
    Task<byte[]> RenderPdfAsync(
        string reportPath,                     // e.g. "/Untitled" or "/bsoft/Untitled"
        IDictionary<string, string?> parameters,
        CancellationToken ct);
}
