using System.Text;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using Microsoft.Extensions.Options;

namespace PurchaseManagement.Infrastructure.Repositories.Reporting;
public sealed class SsrsOptions
{
    public string BaseUrl { get; set; } = "";      // e.g. "http://192.168.1.130/ReportServer"
    public string PoReportPath { get; set; } = ""; // e.g. "/Untitled"
    public SsrsAuthOptions Auth { get; set; } = new();
}

public sealed class SsrsAuthOptions
{
    public bool UseDefaultCredentials { get; set; } = true;
    public string? Domain   { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public sealed class SsrsClient : ISsrsClient
{
    private readonly HttpClient _http;
    private readonly SsrsOptions _opt;

    public SsrsClient(HttpClient http, IOptions<SsrsOptions> opts)
    {
        _http = http;
        _opt = opts.Value;
    }

    public async Task<byte[]> RenderPdfAsync(
        string reportPath,
        IDictionary<string,string?> parameters,
        CancellationToken ct)
        {
            static string E(string s) => Uri.EscapeDataString(s);

            if (!reportPath.StartsWith("/"))
                reportPath = "/" + reportPath;

            var sb = new StringBuilder($"{_opt.BaseUrl}?{E(reportPath)}");

            // ✅ add parameters to URL
            foreach (var kv in parameters.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)))
            {
                sb.Append('&')
                .Append(E(kv.Key))
                .Append('=')
                .Append(E(kv.Value!));
            }

            sb.Append("&rs:Format=PDF");

            using var resp = await _http.GetAsync(sb.ToString(), ct);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsByteArrayAsync(ct);
        }

}
