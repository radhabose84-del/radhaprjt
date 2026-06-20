namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-FR-008a — COA Freeze Engine & DB Triggers (workflow / story).
//
//   As a CFO I want the Chart of Accounts locked at the DB level once frozen, with a visible
//   freeze state and an automatic re-freeze, so no structural change slips through while sealed.
//
// ⚠️ Freezing locks the WHOLE company COA in the shared QA clone, so the freeze step is guarded:
//    it always re-opens an unfreeze window in `finally`, never leaving the COA locked for other tests.
//    Auto-re-freeze (AC3) needs the BSOFT.Api `coa-auto-refreeze` recurring job + a wait → [Fact(Skip)].
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-FR-008a")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-FR-008a")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL02FR008a_CoaFreeze_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa-freeze";

    public US_GL02FR008a_CoaFreeze_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // AC2 / AC4 — banner + DB-trigger-active.
    [Fact, TestPriority(1)]
    public async Task Step1_GetState_ReturnsBannerWithTriggerFlag()
    {
        var resp = await _f.Client.GetAsync($"{Route}/state");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.TryGetProperty("isFrozen", out _).Should().BeTrue();
        data.TryGetProperty("dbTriggerActive", out _).Should().BeTrue("AC4 — enforcement is DB-trigger level");
        data.TryGetProperty("totalAccounts", out _).Should().BeTrue();
        data.TryGetProperty("totalAccountGroups", out _).Should().BeTrue();
    }

    // AC1 + AC4 — while frozen, a structural write is rejected (DB trigger rolled it back).
    // Guarded: always unfreezes in finally so the shared QA COA is not left sealed.
    [Fact, TestPriority(2)]
    public async Task Step2_Frozen_BlocksStructuralWrite_ThenUnfreeze()
    {
        var freeze = await _f.Client.PostAsJsonAsync($"{Route}/set-state", new { isFrozen = true });
        freeze.StatusCode.Should().Be(HttpStatusCode.OK);

        try
        {
            var state = (await ParseAsync(await _f.Client.GetAsync($"{Route}/state"))).RootElement.GetProperty("data");
            state.GetProperty("isFrozen").GetBoolean().Should().BeTrue();
            state.GetProperty("dbTriggerActive").GetBoolean().Should().BeTrue("AC4 — triggers installed");

            // AC1 — a create on a COA table must be blocked by the trigger → friendly 400.
            var code = $"FZ{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(6).ToArray())}";
            var blocked = await _f.Client.PostAsJsonAsync("/api/finance/accountgroup",
                new { groupCode = code, groupName = "Freeze probe", accountTypeId = 1, parentAccountGroupId = (int?)null, sortOrder = 1 });
            blocked.StatusCode.Should().Be(HttpStatusCode.BadRequest, "AC1 — a frozen COA rejects structural writes");
        }
        finally
        {
            // ALWAYS re-open an (effectively permanent) unfreeze window — never leave QA frozen.
            await _f.Client.PostAsJsonAsync($"{Route}/set-state", new { isFrozen = false, unfreezeWindowMinutes = 100000 });
        }
    }

    [Fact(Skip = "AC3 needs BSOFT.Api 'coa-auto-refreeze' Hangfire job running + a real wait for window expiry")]
    [TestPriority(3)]
    public Task Step3_AutoReFreeze_OnWindowExpiry() => Task.CompletedTask;
}
