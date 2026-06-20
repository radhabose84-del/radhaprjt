namespace FinanceManagement.QATests.Tests.CoaFreeze;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-FR-008a — COA Freeze engine (live-server QA).
//
// Route: api/finance/coa-freeze
//   GET  /state                 → freeze banner + status + DB-trigger-active + counts
//   POST /set-state             → TEST/ADMIN seal / open-unfreeze-window (08B replaces it)
//
// AC1/AC4 enforcement is the DB triggers (DBA script) — not asserted from app tests.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CoaFreezeCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CoaFreezeQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa-freeze";

    public CoaFreezeQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetState_HappyPath_Returns200WithBannerFields()
    {
        var resp = await _f.Client.GetAsync($"{Route}/state");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.TryGetProperty("isFrozen", out _).Should().BeTrue();
        data.TryGetProperty("dbTriggerActive", out _).Should().BeTrue();   // verified vs sys.triggers
        data.TryGetProperty("totalAccounts", out _).Should().BeTrue();
        data.TryGetProperty("totalAccountGroups", out _).Should().BeTrue();
    }

    [Fact, TestPriority(2)]
    public async Task TC002_GetState_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/state");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_SetState_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{Route}/set-state", new { isFrozen = false });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
