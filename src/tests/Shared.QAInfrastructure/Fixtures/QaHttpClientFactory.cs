namespace Shared.QAInfrastructure.Fixtures;

// Builds HttpClients that correctly target an app served under a base PATH
// (e.g. http://host/BsoftErpDemo), not just a host root.
//
// The QA tests issue requests with leading-slash paths ("/api/..."), which HttpClient
// resolves against the host ROOT and therefore DROPS any base-path segment — so a BaseUrl
// of http://host/BsoftErpDemo would call http://host/api/... (404). This factory re-inserts
// the base path on every outgoing request via a DelegatingHandler, so the same tests work
// against both root URLs (localhost:5239) and sub-application URLs (host/BsoftErpDemo) with
// no per-test changes. For a root URL the prefix is empty and the handler is a no-op.
internal static class QaHttpClientFactory
{
    public static HttpClient Create(string baseUrl)
    {
        var baseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");
        var prefix  = baseUri.AbsolutePath.TrimEnd('/'); // "" for root, "/BsoftErpDemo" for a sub-app
        var handler = new BasePathHandler(prefix) { InnerHandler = new HttpClientHandler() };
        return new HttpClient(handler) { BaseAddress = baseUri };
    }

    private sealed class BasePathHandler : DelegatingHandler
    {
        private readonly string _prefix;

        public BasePathHandler(string prefix) => _prefix = prefix;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_prefix.Length > 0 && request.RequestUri is { } uri &&
                !uri.AbsolutePath.StartsWith(_prefix + "/", StringComparison.OrdinalIgnoreCase) &&
                !uri.AbsolutePath.Equals(_prefix, StringComparison.OrdinalIgnoreCase))
            {
                request.RequestUri = new UriBuilder(uri) { Path = _prefix + uri.AbsolutePath }.Uri;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
