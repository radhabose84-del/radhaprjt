using System.Net.Http.Json;

namespace PurchaseManagement.Application.External;

// Model that matches Frankfurter /v1/latest
public sealed class FrankLatestResponse
{
    public string @base { get; set; } = default!;
    public string date { get; set; } = default!; // "YYYY-MM-DD"
    public Dictionary<string, decimal> rates { get; set; } = new();
}

public interface IFrankfurterClient
{
    Task<FrankLatestResponse> GetLatestAsync(string baseCcy, IEnumerable<string> symbols, CancellationToken ct);
}

public sealed class FrankfurterClient : IFrankfurterClient
{
    private readonly HttpClient _http;
    public FrankfurterClient(HttpClient http) => _http = http;

    public async Task<FrankLatestResponse> GetLatestAsync(string baseCcy, IEnumerable<string> symbols, CancellationToken ct)
    {
        var url = $"https://api.frankfurter.dev/v1/latest?base={baseCcy}&symbols={string.Join(",", symbols)}";
        var res = await _http.GetFromJsonAsync<FrankLatestResponse>(url, ct);
        if (res is null) throw new InvalidOperationException("Frankfurter returned null.");
        return res;
    }
}
