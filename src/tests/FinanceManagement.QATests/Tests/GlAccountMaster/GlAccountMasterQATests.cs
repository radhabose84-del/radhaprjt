namespace FinanceManagement.QATests.Tests.GlAccountMaster;

// ─────────────────────────────────────────────────────────────────────────────
// GlAccountMaster (live-server QA) — chart-of-accounts GL accounts.
//
// Route: api/finance/glaccountmaster
//   GET (list, paged+search, &AccountTypeId=&AccountGroupId=) · GET {id}
//   GET by-name?term=&AccountTypeCode= · POST · PUT · DELETE ?id=
//
// Create is deep/transactional — it needs a coherent set of FKs (accountTypeId + a LEAF
// accountGroupId + normalBalanceId + currencyTypeId + subLedgerTypeId) plus a format-matched
// accountCode. That cannot be assembled reliably from the QA clone, so create + lifecycle are
// SKIPPED. Smoke / auth / validation / reachability still run live.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("GlAccountMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class GlAccountMasterQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/glaccountmaster";

    public GlAccountMasterQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // ── SMOKE ─────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(Route, new { accountCode = "X", accountName = "X" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── VALIDATION negatives ──────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(Route, new { });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_ByName_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{Route}/by-name?term=QA&AccountTypeCode=1");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── US-GL02-07 type-ahead (search / favourites / recent) ──────────────────
    [Fact, TestPriority(6)]
    [Trait("Layer", "Smoke")]
    public async Task TC006_Search_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}/search?term=1&take=10");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var root = (await ParseAsync(resp)).RootElement;
        root.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Search_EmptyTerm_ReturnsShortcutsOr200()
    {
        // Empty term → favourites + recently-used (or a general TOP-N when the user has none).
        var resp = await _f.Client.GetAsync($"{Route}/search");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Favourites_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{Route}/favourites");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Search_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/search?term=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_AddFavourite_BadId_Returns400()
    {
        // Validator: GlAccountMasterId must be > 0 and exist.
        var resp = await _f.Client.PostAsJsonAsync($"{Route}/favourites", new { glAccountMasterId = 0 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── SKIP — deep create + lifecycle (seed-dependent) ───────────────────────
    [Fact(Skip = "needs seeded data: AccountType + leaf AccountGroup + normalBalance + currencyType + subLedgerType + format-matched accountCode")]
    [TestPriority(10)]
    public async Task TC010_Create_FullLifecycle()
    {
        await Task.CompletedTask;
    }
}
