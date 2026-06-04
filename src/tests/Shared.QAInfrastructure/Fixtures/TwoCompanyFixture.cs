namespace Shared.QAInfrastructure.Fixtures;

// Logs in TWO users bound to DIFFERENT companies so cross-company data-isolation (IDOR)
// can be asserted: ClientA (Company A) must never be able to read/modify ClientB's data
// and vice-versa.
//
// The two users do not exist in the QA DB until provisioned. Until then this fixture
// stays in an un-configured state (IsConfigured = false) and does NOT attempt login, so
// it never throws and never reds the suite — the isolation tests are Skipped meanwhile.
//
// To enable: create two users on two companies, set
//   QAServer:Isolation:CompanyA:{Username,Password,CompanyId}
//   QAServer:Isolation:CompanyB:{Username,Password,CompanyId}
// in appsettings.QA.json (or env QAServer__Isolation__CompanyA__Username, …), and remove
// the [Skip] on the isolation tests.
public sealed class TwoCompanyFixture : IAsyncLifetime
{
    private readonly IConfiguration _config;

    public HttpClient ClientA { get; private set; } = null!;
    public HttpClient ClientB { get; private set; } = null!;

    public int CompanyAId { get; private set; }
    public int CompanyBId { get; private set; }

    public string EntityCode { get; set; } = string.Empty;
    public int CreatedId { get; set; }

    /// True only when both isolation users are configured AND both logins succeeded.
    public bool IsConfigured { get; private set; }

    public TwoCompanyFixture()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.QA.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public async Task InitializeAsync()
    {
        // Run-unique token (same scheme as QAServerFixture).
        var ticks = DateTimeOffset.UtcNow.Ticks.ToString();
        EntityCode = "Q" + new string(ticks.Reverse().ToArray());

        var baseUrl = _config["QAServer:BaseUrl"];
        var aUser   = _config["QAServer:Isolation:CompanyA:Username"];
        var aPass   = _config["QAServer:Isolation:CompanyA:Password"];
        var bUser   = _config["QAServer:Isolation:CompanyB:Username"];
        var bPass   = _config["QAServer:Isolation:CompanyB:Password"];
        int.TryParse(_config["QAServer:Isolation:CompanyA:CompanyId"], out var aId);
        int.TryParse(_config["QAServer:Isolation:CompanyB:CompanyId"], out var bId);
        CompanyAId = aId;
        CompanyBId = bId;

        // Not provisioned yet → stay un-configured (tests Skip), never throw.
        if (string.IsNullOrWhiteSpace(baseUrl) ||
            string.IsNullOrWhiteSpace(aUser) || string.IsNullOrWhiteSpace(bUser))
        {
            return;
        }

        ClientA = await LoginAsync(baseUrl!, aUser!, aPass ?? string.Empty);
        ClientB = await LoginAsync(baseUrl!, bUser!, bPass ?? string.Empty);
        IsConfigured = true;
    }

    private static async Task<HttpClient> LoginAsync(string baseUrl, string username, string password)
    {
        var anon = new HttpClient { BaseAddress = new Uri(baseUrl) };
        await anon.PostAsJsonAsync("/api/auth/deactivate-user-sessionByUsername",
            new { username, password });

        var resp = await anon.PostAsJsonAsync("/api/auth/login", new { username, password });
        resp.EnsureSuccessStatusCode();

        var doc   = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("token").GetString()!;

        var client = new HttpClient { BaseAddress = new Uri(baseUrl) };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public Task DisposeAsync()
    {
        ClientA?.Dispose();
        ClientB?.Dispose();
        return Task.CompletedTask;
    }
}
