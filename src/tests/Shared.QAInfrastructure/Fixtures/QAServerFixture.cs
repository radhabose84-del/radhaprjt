namespace Shared.QAInfrastructure.Fixtures;

public sealed class QAServerFixture : IAsyncLifetime
{
    private readonly IConfiguration _config;

    public HttpClient Client { get; private set; } = null!;
    public HttpClient AnonymousClient { get; private set; } = null!;

    public string BaseUrl { get; private set; } = string.Empty;

    // The QA login the fixture authenticates with — exposed so auth-focused suites can re-login
    // with the real configured credentials instead of hard-coding them.
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;

    // Shared state — set by TC001, used by all subsequent tests
    public int CreatedId { get; set; }

    // Secondary shared state — e.g. role assignment id
    public int SecondaryId { get; set; }

    // Config values accessible by test classes
    public string EntityCode { get; set; } = string.Empty;
    public int ValidRoleId { get; private set; }
    public int ValidValueId { get; private set; }

    // A real, existing City Id resolved at runtime (the QA clone has no City with Id=1).
    // Used by payloads with a City FK (e.g. Company address) so they don't assume a seed Id.
    public int CityId { get; private set; }

    // Real, ACTIVE cross-module reference ids resolved at runtime from the clone's existing
    // master data (the reset only deletes testsales rows, so prod-clone reference data survives).
    // These let create-happy tests use a genuinely-valid FK instead of a hard-coded id 1 (which
    // is often inactive/deleted on the clone). 0 => none resolvable → dependent tests self-skip.
    public int ActiveItemId { get; private set; }       // InventoryManagement item (itemId FK)
    public int CustomerPartyId { get; private set; }    // PartyManagement party of type CUSTOMER
    public int AgentPartyId { get; private set; }       // PartyManagement party of type AGENT

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
        Username = username;
        Password = password;
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

        AnonymousClient = QaHttpClientFactory.Create(BaseUrl);

        // Force logout — clear any existing session
        await AnonymousClient.PostAsJsonAsync(
            "/api/auth/deactivate-user-sessionByUsername",
            new { username, password });

        // Login — capture token
        var loginResp = await AnonymousClient.PostAsJsonAsync(
            "/api/auth/login",
            new { username, password });

        // Single-session guard: `testsales` allows only one active session, so a lingering session
        // (a prior crashed/aborted run, a manual probe, or a slow deactivate) makes login return
        // 400 "already logged in on another machine." Deactivate + retry a few times so a stale
        // session self-heals instead of failing the whole collection at init.
        for (var attempt = 0; attempt < 4 && loginResp.StatusCode == HttpStatusCode.BadRequest; attempt++)
        {
            await AnonymousClient.PostAsJsonAsync(
                "/api/auth/deactivate-user-sessionByUsername",
                new { username, password });
            await Task.Delay(750);
            loginResp = await AnonymousClient.PostAsJsonAsync(
                "/api/auth/login",
                new { username, password });
        }

        loginResp.StatusCode.Should().Be(HttpStatusCode.OK,
            "QA server login must succeed before tests run.");

        var doc   = JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("data").GetProperty("token").GetString()!;

        Client = QaHttpClientFactory.Create(BaseUrl);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        // Resolve a real City Id for FK-dependent payloads. Guarded so a lookup hiccup never
        // fails the whole collection — dependent tests will surface a clear failure instead.
        try
        {
            var cityResp = await Client.GetAsync("/api/City?PageNumber=1&PageSize=1");
            if (cityResp.IsSuccessStatusCode)
            {
                using var cityDoc = JsonDocument.Parse(await cityResp.Content.ReadAsStringAsync());
                if (cityDoc.RootElement.TryGetProperty("data", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
                {
                    CityId = arr[0].GetProperty("id").GetInt32();
                }
            }
        }
        catch { /* leave CityId = 0; City-FK tests will report a clear failure */ }

        // Resolve a real ACTIVE Inventory item id (prefer isActive==1). Guarded — item-FK tests
        // self-skip when this stays 0.
        try
        {
            var itemResp = await Client.GetAsync("/api/ItemMaster?PageNumber=1&PageSize=25");
            if (itemResp.IsSuccessStatusCode)
            {
                using var itemDoc = JsonDocument.Parse(await itemResp.Content.ReadAsStringAsync());
                if (itemDoc.RootElement.TryGetProperty("data", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var it in arr.EnumerateArray())
                    {
                        var isActive = it.TryGetProperty("isActive", out var ia)
                            && (ia.ValueKind == JsonValueKind.True
                                || (ia.ValueKind == JsonValueKind.Number && ia.GetInt32() == 1));
                        if (isActive && it.TryGetProperty("id", out var idp))
                        {
                            ActiveItemId = idp.GetInt32();
                            break;
                        }
                    }
                    if (ActiveItemId == 0 && arr.GetArrayLength() > 0
                        && arr[0].TryGetProperty("id", out var firstId))
                        ActiveItemId = firstId.GetInt32();
                }
            }
        }
        catch { /* leave ActiveItemId = 0 */ }

        // Resolve a real CUSTOMER party id and a distinct AGENT party id by scanning the
        // partyTypes[].partyTypeName on each party row. Guarded — party-FK tests self-skip at 0.
        try
        {
            var partyResp = await Client.GetAsync("/api/party/PartyMaster?PageNumber=1&PageSize=50");
            if (partyResp.IsSuccessStatusCode)
            {
                using var partyDoc = JsonDocument.Parse(await partyResp.Content.ReadAsStringAsync());
                if (partyDoc.RootElement.TryGetProperty("data", out var arr) && arr.ValueKind == JsonValueKind.Array)
                {
                    foreach (var p in arr.EnumerateArray())
                    {
                        if (!p.TryGetProperty("id", out var idp)) continue;
                        var pid = idp.GetInt32();
                        if (!p.TryGetProperty("partyTypes", out var pts) || pts.ValueKind != JsonValueKind.Array) continue;

                        foreach (var t in pts.EnumerateArray())
                        {
                            var name = t.TryGetProperty("partyTypeName", out var n) ? n.GetString() : null;
                            if (name == "CUSTOMER" && CustomerPartyId == 0) CustomerPartyId = pid;
                            if (name == "AGENT" && AgentPartyId == 0) AgentPartyId = pid;
                        }
                        if (CustomerPartyId > 0 && AgentPartyId > 0 && CustomerPartyId != AgentPartyId) break;
                    }
                }
            }
        }
        catch { /* leave Customer/AgentPartyId = 0 */ }
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
