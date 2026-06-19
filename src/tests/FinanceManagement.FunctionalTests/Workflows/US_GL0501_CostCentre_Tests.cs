namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL05-01 — Cost Centre Master & 3-level hierarchy (workflow / story).
//
//   As a Finance Controller, I maintain a unit-wise cost-centre master with a
//   Plant (L1) → Department Group (L2) → Department (L3) hierarchy, so costs can be
//   tagged and rolled up for departmental reporting.
//
// Proves the MASTER story (QA covers individual endpoints): an L1 Plant is created,
// renamed, and deactivated — deactivation hides it from the parent picker (autocomplete)
// but keeps it in GetAll (not deleted). The enforcement/rollup criteria (AC#3 deactivation
// blocked by open transactions, AC#4 manager-change alert routing, AC#5 rollup totals)
// depend on the journal engine (Sprint 2) / reporting (FR-004) and are [Fact(Skip=...)].
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL05-01")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL05-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0501_CostCentre_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/CostCentre";

    // Cross-step state (collection runs steps serially).
    private static int _id;
    private static int _l1Level;
    private static string _code = string.Empty;

    public US_GL0501_CostCentre_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private async Task<int> ResolveLevelIdAsync(string levelCode)
    {
        var resp = await _f.Client.GetAsync("/api/finance/miscmaster/by-name?MiscTypeCode=COSTCENTRELEVEL");
        if (resp.StatusCode != HttpStatusCode.OK) return 0;
        var root = (await ParseAsync(resp)).RootElement;
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) return 0;
        foreach (var item in data.EnumerateArray())
        {
            var code = item.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() : null;
            if (string.Equals(code, levelCode, StringComparison.OrdinalIgnoreCase) &&
                item.TryGetProperty("id", out var idEl) && idEl.ValueKind == JsonValueKind.Number)
                return idEl.GetInt32();
        }
        return 0;
    }

    // STEP 1 — a created L1 Plant is immediately available to the parent-CC picker (autocomplete).
    [Fact, TestPriority(1)]
    public async Task Step1_CreatePlant_BecomesAvailableInParentPicker()
    {
        _l1Level = await ResolveLevelIdAsync("CCL1");
        if (_l1Level == 0) return;   // self-skip: COSTCENTRELEVEL not seeded in this clone

        _code = $"FT{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(14).ToArray())}";

        var createResp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            costCentreCode = _code,
            costCentreName = $"FT Plant {_f.EntityCode}",
            centreLevelId = _l1Level,
            parentCostCentreId = (int?)null
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(createResp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);

        var byNameResp = await _f.Client.GetAsync($"{Route}/by-name?term={_code}&level={_l1Level}");
        byNameResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await ParseAsync(byNameResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("costCentreCode").GetString());
        codes.Should().Contain(_code, "a new L1 Plant is offered to the parent-CC picker for L2 children");
    }

    // STEP 2 — editing the name is reflected (code/level/plant are immutable).
    [Fact, TestPriority(2)]
    public async Task Step2_EditCostCentreName()
    {
        if (_id == 0) return;   // Step1 self-skipped

        var updResp = await _f.Client.PutAsJsonAsync($"{Route}", new
        {
            id = _id,
            costCentreName = $"FT Plant Edited {_f.EntityCode}",
            isActive = 1
        });
        updResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var byId = await _f.Client.GetAsync($"{Route}/{_id}");
        var name = (await ParseAsync(byId)).RootElement.GetProperty("data").GetProperty("costCentreName").GetString();
        name.Should().Be($"FT Plant Edited {_f.EntityCode}");
    }

    // STEP 3 (Deactivate) — inactive CC drops out of the picker but stays in GetAll (not deleted).
    [Fact, TestPriority(3)]
    public async Task Step3_Deactivate_ExcludesFromPicker_ButKeepsInGetAll()
    {
        if (_id == 0) return;   // Step1 self-skipped

        var deactResp = await _f.Client.PutAsJsonAsync($"{Route}", new
        {
            id = _id,
            costCentreName = $"FT Plant Edited {_f.EntityCode}",
            isActive = 0
        });
        deactResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var byName = await _f.Client.GetAsync($"{Route}/by-name?term={_code}&level={_l1Level}");
        var codes = (await ParseAsync(byName)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("costCentreCode").GetString());
        codes.Should().NotContain(_code, "deactivated cost centres are hidden from the parent picker");

        var allResp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=200&SearchTerm={_code}");
        var allCodes = (await ParseAsync(allResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("costCentreCode").GetString());
        allCodes.Should().Contain(_code, "deactivation is not deletion — record is retained");
    }

    // STEP 4 (enforcement / rollup) — depends on the journal engine (Sprint 2) and reporting (FR-004).
    [Fact(Skip = "Blocked — Sprint 2 / FR-004: AC#3 block deactivation when open transactions exist " +
                 "(needs the journal/production-order engine; HasOpenTransactionsAsync is a stub today), " +
                 "AC#4 manager-change updates budget-alert routing (needs the Budget consumer), " +
                 "AC#5 department costs roll up to division & plant totals (reporting). None exist yet."),
     TestPriority(4)]
    public async Task Step4_EnforcementAndRollup_AreSprint2()
    {
        await Task.CompletedTask;
    }
}
