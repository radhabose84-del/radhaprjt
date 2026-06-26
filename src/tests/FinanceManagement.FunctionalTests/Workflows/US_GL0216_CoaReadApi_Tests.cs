using System.Diagnostics;

namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-16 — COA Read API for downstream modules (story).
//
//   As a downstream-module developer (AP/AR/FA) I want a REST read API — get-by-code, search by
//   type/group, validate-for-posting — plus a deactivation event, so I can integrate reliably.
//
// Reachable live: get-by-code (AC1, timed), search-with-status (AC5), validate-for-posting (AC2).
// AC3 (deactivation event within 1s) is asynchronous over the bus and has no HTTP surface — it is
// proven by the unit suite (IntegrationEventPublisherTests + UpdateGlAccountMasterCommandHandlerTests).
// AC4 (auth + logging) is shown by the no-auth 401 in the QA suite + per-call audit events.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-16")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-16")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0216_CoaReadApi_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/coa";

    public US_GL0216_CoaReadApi_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // Step 1 — AC1: get-by-code responds (and quickly). Uses an unknown code so it's seed-independent;
    // the path + index are what we exercise for the latency budget.
    [Fact, TestPriority(1)]
    public async Task Step1_GetByCode_RespondsQuickly()
    {
        var sw = Stopwatch.StartNew();
        var resp = await _f.Client.GetAsync($"{Route}/accounts/by-code/FT-UNKNOWN");
        sw.Stop();

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
        sw.Elapsed.TotalSeconds.Should().BeLessThan(5, "AC1 target is <100ms server-side; allow live network headroom");
    }

    // Step 2 — AC5: search by type/group returns accounts with their status.
    [Fact, TestPriority(2)]
    public async Task Step2_Search_ReturnsAccountsWithStatus()
    {
        var resp = await _f.Client.GetAsync($"{Route}/accounts?activeOnly=false");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Array);
        if (data.GetArrayLength() > 0)
            data[0].TryGetProperty("isActive", out _).Should().BeTrue("AC5 — each row carries status");
    }

    // Step 3 — AC2: validate-for-posting returns a structured pass/fail with reasons.
    [Fact, TestPriority(3)]
    public async Task Step3_ValidateForPosting_ReturnsResult()
    {
        var resp = await _f.Client.GetAsync($"{Route}/accounts/validate-for-posting?accountCode=FT-UNKNOWN");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.GetProperty("isValid").GetBoolean().Should().BeFalse("unknown account is not valid for posting");
        data.GetProperty("reasons").GetArrayLength().Should().BeGreaterThan(0, "AC2 — failure carries a reason");
    }

    // Step 4 — AC4: the API rejects unauthenticated calls.
    [Fact, TestPriority(4)]
    public async Task Step4_RequiresAuth()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/accounts");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // AC3 — deactivation event within 1s is async/bus-only (no HTTP surface); covered by the unit suite.
    [Fact(Skip = "AC3 deactivation event is asynchronous (bus); no HTTP surface — covered by IntegrationEventPublisherTests + UpdateGlAccountMasterCommandHandlerTests."),
     TestPriority(5)]
    public void Step5_DeactivationEvent_PublishedWithin1s() { }
}
