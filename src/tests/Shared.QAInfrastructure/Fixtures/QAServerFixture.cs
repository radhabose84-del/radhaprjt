namespace Shared.QAInfrastructure.Fixtures;

public sealed class QAServerFixture : IAsyncLifetime
{
    private readonly IConfiguration _config;

    public HttpClient Client { get; private set; } = null!;
    public HttpClient AnonymousClient { get; private set; } = null!;

    public string BaseUrl { get; private set; } = string.Empty;

    // Shared state — set by TC001, used by all subsequent tests
    public int CreatedId { get; set; }

    // Secondary shared state — e.g. role assignment id
    public int SecondaryId { get; set; }

    // Config values accessible by test classes
    public string EntityCode { get; set; } = string.Empty;
    public int ValidRoleId { get; private set; }
    public int ValidValueId { get; private set; }

    public QAServerFixture()
    {
        // appsettings.QA.json holds local defaults (localhost:5239). Environment variables
        // override them so CI can retarget the deployed QA app without editing the file —
        // e.g. QAServer__BaseUrl=http://198.168.1.130/BsoftErp (also __Username, __Password).
        _config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.QA.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public async Task InitializeAsync()
    {
        BaseUrl  = _config["QAServer:BaseUrl"]!;
        var username = _config["QAServer:Username"]!;
        var password = _config["QAServer:Password"]!;
        ValidRoleId  = int.Parse(_config["QAServer:ValidRoleId"]!);
        ValidValueId = int.Parse(_config["QAServer:ValidValueId"]!);

        // Tests slice EntityCode[..N] (N up to 10) for unique code fields, so the PREFIX
        // must change every run — otherwise creates on unique-code entities (Country, State,
        // Currency, Unit, …) hit "already exists" 400s and cascade the whole collection.
        // The old "QA{unixSeconds}" used the unchanging high-order timestamp digits as the
        // prefix. Reversing the high-resolution tick count puts the fastest-varying digits
        // first, so every prefix length is run-unique. Letter-led to keep it a clean code.
        var ticks = DateTimeOffset.UtcNow.Ticks.ToString();
        EntityCode = "Q" + new string(ticks.Reverse().ToArray());

        AnonymousClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };

        // Force logout — clear any existing session
        await AnonymousClient.PostAsJsonAsync(
            "/api/auth/deactivate-user-sessionByUsername",
            new { username, password });

        // Login — capture token
        var loginResp = await AnonymousClient.PostAsJsonAsync(
            "/api/auth/login",
            new { username, password });

        loginResp.StatusCode.Should().Be(HttpStatusCode.OK,
            "QA server login must succeed before tests run.");

        var doc   = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("token").GetString()!;

        Client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync()
    {
        // Null-guarded: if InitializeAsync threw before these were assigned, disposing
        // here must not raise a NullReferenceException that masks the real failure.
        Client?.Dispose();
        AnonymousClient?.Dispose();
        return Task.CompletedTask;
    }
}
