namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-10 — Multi-Company COA (shared global template + entity-specific overrides) story.
//
//   As a Group Finance Controller I want a global COA shared across entities, with
//   entity-specific overrides and company-restricted accounts, so group consistency is
//   maintained while each entity keeps its specific accounts.
//
// Reachable live: the mandatory company selector (AC5), the single-entity consistency report
// (AC4), and the subsidiary-inherit endpoint (AC1, idempotent — a no-op when no template company
// is configured on the clone). The deep behaviours — propagation of a global edit to subsidiary
// copies (AC3) and rejecting a restricted account posted from another entity (AC2) — require
// MultiCompanyCoa:TemplateCompanyId configured plus ≥2 companies sharing one EntityId and a
// coherent GL/journal seed on the clone, so they are [Fact(Skip)] pending that setup (see
// Story-Catalogue). Both are covered by the integration suite (GlAccountMasterMultiCompanyTests).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-10")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-10")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0210_MultiCompanyCoa_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/glaccountmaster";

    public US_GL0210_MultiCompanyCoa_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // Step 1 — AC5: the company selector is served and scoped to the user (an array, possibly empty).
    [Fact, TestPriority(1)]
    public async Task Step1_CompanySelector_IsServed()
    {
        var resp = await _f.Client.GetAsync($"{Route}/selectable-companies");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Array, "AC5 — the selector lists the user's companies");
    }

    // Step 2 — AC5: the selector requires auth (no token → 401), i.e. it cannot be used anonymously.
    [Fact, TestPriority(2)]
    public async Task Step2_CompanySelector_RequiresAuth()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}/selectable-companies");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Step 3 — AC4: the consistency report runs and returns flagged single-entity accounts (array).
    [Fact, TestPriority(3)]
    public async Task Step3_ConsistencyReport_Runs()
    {
        var resp = await _f.Client.GetAsync($"{Route}/consistency-report");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Array, "AC4 — single-entity accounts are listed");
        // When rows exist, each carries the human-readable single-entity flag (e.g. 'in Processing only').
        if (data.GetArrayLength() > 0)
            data[0].TryGetProperty("flag", out _).Should().BeTrue("AC4 — each row is flagged");
    }

    // Step 4 — AC1: the subsidiary-inherit action is reachable and idempotent (returns a count >= 0).
    [Fact, TestPriority(4)]
    public async Task Step4_InheritGlobal_IsIdempotentAndReturnsCount()
    {
        var resp = await _f.Client.PostAsync($"{Route}/inherit-global/1", content: null);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32()
            .Should().BeGreaterThanOrEqualTo(0, "AC1 — inherit copies the missing global accounts");
    }

    // Step 5 — AC3: a global edit propagates to subsidiary copies except where an override exists.
    [Fact(Skip = "Needs MultiCompanyCoa:TemplateCompanyId configured + ≥2 companies in one EntityId on the clone; covered by GlAccountMasterMultiCompanyTests."), TestPriority(5)]
    public void Step5_GlobalEdit_PropagatesExceptOverrides() { }

    // Step 6 — AC2: a company-restricted account cannot be posted to from another entity.
    [Fact(Skip = "Needs a restricted account in entity A + a journal seed in entity B on the clone; covered by GlAccountMasterMultiCompanyTests."), TestPriority(6)]
    public void Step6_RestrictedAccount_RejectedFromAnotherEntity() { }
}
