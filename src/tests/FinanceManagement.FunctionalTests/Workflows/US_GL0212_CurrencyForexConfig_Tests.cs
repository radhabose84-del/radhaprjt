namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-12 — Account Currency & Forex Configuration (workflow / story).
//
//   As a Finance Controller, I maintain the currency-type master so the GL Account
//   screen's single "Currency Type" dropdown draws from one governed list (INR-only /
//   Forex / Multi-currency, …) instead of free-form values.
//
// Proves the CONFIGURATION story (QA covers individual endpoints). Create/edit/deactivate
// are self-seeding and run live. The enforcement criteria (AC1–AC5: reject FC posting to
// INR-only, permit forex, revaluation routing, EEFC report, lock-after-posting) depend on
// the GL-04 posting/revaluation engine and are [Fact(Skip=...)] — never a silent gap.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-12")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-12")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0212_CurrencyForexConfig_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/CurrencyForexConfig";

    // Cross-step state (collection runs steps serially).
    private static int _id;
    private static string _code = string.Empty;

    public US_GL0212_CurrencyForexConfig_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // STEP 1 — a created currency type is immediately available to the GL "Currency Type" dropdown.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateCurrencyType_BecomesAvailableInDropdown()
    {
        // CurrencyTypeCode has a 20-char limit; "FT" + EntityCode (~20) overflows → clamp to 20.
        _code = ("FT" + _f.EntityCode)[..Math.Min(20, ("FT" + _f.EntityCode).Length)];

        var createResp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            currencyTypeCode = _code,
            currencyTypeName = $"FT Type {_f.EntityCode}"
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(createResp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);

        // Available in the autocomplete the GL Account screen's dropdown consumes.
        var byNameResp = await _f.Client.GetAsync($"{Route}/by-name?term={_code}");
        byNameResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await ParseAsync(byNameResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("currencyTypeCode").GetString());
        codes.Should().Contain(_code, "a new currency type is offered to the GL dropdown with no code change");
    }

    // STEP 2 — editing the name is reflected (code is immutable).
    [Fact, TestPriority(2)]
    public async Task Step2_EditCurrencyType()
    {
        _id.Should().BeGreaterThan(0, "Step1 must have created it");

        var updResp = await _f.Client.PutAsJsonAsync($"{Route}", new
        {
            id = _id,
            currencyTypeName = $"FT Edited {_f.EntityCode}",
            isActive = 1
        });
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var byId = await _f.Client.GetAsync($"{Route}/{_id}");
        var name = (await ParseAsync(byId)).RootElement.GetProperty("data").GetProperty("currencyTypeName").GetString();
        name.Should().Be($"FT Edited {_f.EntityCode}");
    }

    // STEP 3 (Deactivate) — inactive type drops out of the dropdown but stays in GetAll (not deleted).
    [Fact, TestPriority(3)]
    public async Task Step3_Deactivate_ExcludesFromDropdown_ButKeepsInGetAll()
    {
        _id.Should().BeGreaterThan(0);

        var deactResp = await _f.Client.PutAsJsonAsync($"{Route}", new
        {
            id = _id,
            currencyTypeName = $"FT Edited {_f.EntityCode}",
            isActive = 0
        });
        deactResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Excluded from active autocomplete (the dropdown).
        var byName = await _f.Client.GetAsync($"{Route}/by-name?term={_code}");
        var codes = (await ParseAsync(byName)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("currencyTypeCode").GetString());
        codes.Should().NotContain(_code, "deactivated types are hidden from the dropdown");

        // Still present (not deleted) in GetAll.
        var allResp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=200&SearchTerm={_code}");
        var allCodes = (await ParseAsync(allResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("currencyTypeCode").GetString());
        allCodes.Should().Contain(_code, "deactivation is not deletion — record is retained");
    }

    // STEP 4 (enforcement) — AC1–AC5 depend on the GL-04 posting/revaluation engine.
    [Fact(Skip = "Blocked — GL-04 (Sprint 2): AC1 reject foreign-currency posting to an INR-only account, " +
                 "AC2 permit USD/EUR on a forex account, AC3 revaluation routes to the configured forex gain/loss account, " +
                 "AC4 EEFC balance report (FEMA), AC5 lock currency type after first posting. " +
                 "All need the posting/revaluation engine which does not exist yet."),
     TestPriority(4)]
    public async Task Step4_Enforcement_IsGl04()
    {
        await Task.CompletedTask;
    }
}
