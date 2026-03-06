using BackgroundService.Application.Interfaces.Files;

namespace BackgroundService.Infrastructure.Files
{
    public sealed class HttpFileFetcher : IFileFetcher
    {
        private readonly HttpClient _http;
        public HttpFileFetcher(HttpClient http) => _http = http;

        public async Task<byte[]> FetchAsync(string urlOrPath, CancellationToken ct)
        {
            // Support UNC/local paths as well as http(s)
            if (Uri.TryCreate(urlOrPath, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                using var resp = await _http.GetAsync(uri, ct);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsByteArrayAsync(ct);
            }

            // Treat as file path
            return await File.ReadAllBytesAsync(urlOrPath, ct);
        }
    }
}
