namespace FinanceManagement.QATests.Tests.CostCentre;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL05-01 — Cost Centre master & 3-level hierarchy (live-server QA).
//
// Route: api/finance/CostCentre
//   GET (list, paged+search) · GET {id} · GET by-name?term=&level= (autocomplete / parent picker)
//   POST · PUT · DELETE ?id=   (UnitId+CompanyId from the token; CostCentreCode immutable+alphanumeric,
//   unique per unit; CentreLevelId → Finance.MiscMaster 'COSTCENTRELEVEL')
//
// Level ids differ per environment → resolved at runtime from miscmaster/by-name?MiscTypeCode=COSTCENTRELEVEL.
// The L1 (Plant) lifecycle is self-contained (no cross-module FK) and runs live. The L2 hierarchy step
// resolves a Department Group at runtime and self-skips if the QA clone has none.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CostCentreCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CostCentreQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/CostCentre";

    public CostCentreQATests(QAServerFixture fixture) => _f = fixture;

    // Code: alphanumeric only, <= 20 chars, unique per unit.
    private string UniqueCode(string suffix = "") =>
        $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(14).ToArray())}{suffix}";

    private static string? TryStr(JsonElement e, string p) =>
        e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static int TryInt(JsonElement e, string p) =>
        e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    // Resolve a COSTCENTRELEVEL misc id by its code (CCL1 / CCL2 / CCL3). 0 if not seeded.
    private async Task<int> ResolveLevelIdAsync(string levelCode)
    {
        var resp = await _f.Client.GetAsync("/api/finance/miscmaster/by-name?MiscTypeCode=COSTCENTRELEVEL");
        if (resp.StatusCode != HttpStatusCode.OK) return 0;

        var root = (await QAHelper.ParseAsync(resp)).RootElement;
        if (!root.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) return 0;

        foreach (var item in data.EnumerateArray())
        {
            var code = TryStr(item, "code") ?? TryStr(item, "miscCode");
            if (string.Equals(code, levelCode, StringComparison.OrdinalIgnoreCase))
            {
                var id = TryInt(item, "id");
                if (id == 0) id = TryInt(item, "miscId");
                return id;
            }
        }
        return 0;
    }

    private object L1Body(string code) => new
    {
        costCentreCode = code,
        costCentreName = $"QA Plant {_f.EntityCode}",
        centreLevelId = 0,           // overwritten by caller
        parentCostCentreId = (int?)null,
        departmentGroupId = (int?)null,
        departmentId = (int?)null
    };

    // ── SMOKE — login → auth → DB → read works (deploy gate) ──────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task CC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await QAHelper.ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task CC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task CC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{Route}", new { costCentreCode = UniqueCode(), centreLevelId = 1 });
        await QAHelper.Assert401Async(resp);
    }

    // ── VALIDATION negatives ──────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task CC004_Create_MissingRequired_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{Route}", new { costCentreCode = "", costCentreName = "", centreLevelId = 0 });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task CC005_Create_NonAlphanumericCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{Route}",
            new { costCentreCode = "QA-01", costCentreName = "Bad Code", centreLevelId = 1, parentCostCentreId = (int?)null });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task CC006_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{Route}", new { id = 0, costCentreName = "", isActive = 1 });
        await QAHelper.Assert400Async(resp);
    }

    // ── FULL L1 LIFECYCLE (self-contained; only needs the COSTCENTRELEVEL L1 id) ──
    [Fact, TestPriority(10)]
    public async Task CC010_FullL1Lifecycle()
    {
        var l1Level = await ResolveLevelIdAsync("CCL1");
        if (l1Level == 0) return;    // self-skip: COSTCENTRELEVEL not seeded in this clone

        var code = UniqueCode();

        // Create L1
        var createResp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            costCentreCode = code,
            costCentreName = $"QA Plant {_f.EntityCode}",
            centreLevelId = l1Level,
            parentCostCentreId = (int?)null,
            departmentGroupId = (int?)null,
            departmentId = (int?)null
        });
        await QAHelper.AssertOkAsync(createResp);
        var id = await QAHelper.GetCreatedIdAsync(createResp);
        id.Should().BeGreaterThan(0);

        // Duplicate code in same unit → 400
        var dupResp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            costCentreCode = code,
            costCentreName = $"QA Plant Dup {_f.EntityCode}",
            centreLevelId = l1Level,
            parentCostCentreId = (int?)null
        });
        await QAHelper.Assert400Async(dupResp);

        // GET by id
        (await _f.Client.GetAsync($"{Route}/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        // by-name autocomplete (filtered to L1) contains it
        var byNameResp = await _f.Client.GetAsync($"{Route}/by-name?term={code}&level={l1Level}");
        byNameResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await QAHelper.ParseAsync(byNameResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("costCentreCode").GetString());
        codes.Should().Contain(code);

        // Update (rename)
        var updResp = await _f.Client.PutAsJsonAsync($"{Route}",
            new { id, costCentreName = $"QA Plant Edited {_f.EntityCode}", isActive = 1 });
        await QAHelper.AssertOkAsync(updResp);

        // Deactivate → hidden from autocomplete, kept in GetAll
        var deactResp = await _f.Client.PutAsJsonAsync($"{Route}",
            new { id, costCentreName = $"QA Plant Edited {_f.EntityCode}", isActive = 0 });
        await QAHelper.AssertOkAsync(deactResp);

        var byName2 = await _f.Client.GetAsync($"{Route}/by-name?term={code}&level={l1Level}");
        var codes2 = (await QAHelper.ParseAsync(byName2)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("costCentreCode").GetString());
        codes2.Should().NotContain(code);

        // Delete (soft) — no children/open-txns, succeeds
        var delResp = await _f.Client.DeleteAsync($"{Route}?id={id}");
        await QAHelper.AssertOkAsync(delResp);
    }

    // ── HIERARCHY — L2 under an L1, plant inherited (needs a Department Group) ──
    [Fact, TestPriority(20)]
    public async Task CC020_L2_Under_L1_InheritsPlant()
    {
        var l1Level = await ResolveLevelIdAsync("CCL1");
        var l2Level = await ResolveLevelIdAsync("CCL2");
        if (l1Level == 0 || l2Level == 0) return;   // self-skip: levels not seeded

        // Resolve a department group at runtime; self-skip if the clone has none.
        var dgResp = await _f.Client.GetAsync("/api/departmentgroup?PageNumber=1&PageSize=1");
        if (dgResp.StatusCode != HttpStatusCode.OK) return;
        var dgRoot = (await QAHelper.ParseAsync(dgResp)).RootElement;
        if (!dgRoot.TryGetProperty("data", out var dgData) || dgData.ValueKind != JsonValueKind.Array || dgData.GetArrayLength() == 0) return;
        var deptGroupId = TryInt(dgData[0], "id");
        if (deptGroupId == 0) deptGroupId = TryInt(dgData[0], "departmentGroupId");
        if (deptGroupId == 0) return;

        // Create the L1 parent
        var l1Resp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            costCentreCode = UniqueCode("P"),
            costCentreName = $"QA Plant {_f.EntityCode}",
            centreLevelId = l1Level,
            parentCostCentreId = (int?)null
        });
        await QAHelper.AssertOkAsync(l1Resp);
        var l1Id = await QAHelper.GetCreatedIdAsync(l1Resp);

        // Create the L2 under it
        var l2Resp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            costCentreCode = UniqueCode("D"),
            costCentreName = $"QA Dept Group {_f.EntityCode}",
            centreLevelId = l2Level,
            parentCostCentreId = l1Id,
            departmentGroupId = deptGroupId
        });
        await QAHelper.AssertOkAsync(l2Resp);
        var l2Id = await QAHelper.GetCreatedIdAsync(l2Resp);

        // L2 row shows its parent name (plant inherited up the tree)
        var byId = await _f.Client.GetAsync($"{Route}/{l2Id}");
        var data = (await QAHelper.ParseAsync(byId)).RootElement.GetProperty("data");
        data.GetProperty("parentCostCentreId").GetInt32().Should().Be(l1Id);
        data.GetProperty("parentCostCentreName").GetString().Should().NotBeNullOrEmpty();

        // cleanup leaf-first
        await _f.Client.DeleteAsync($"{Route}?id={l2Id}");
        await _f.Client.DeleteAsync($"{Route}?id={l1Id}");
    }

    // ── AC#3 deactivation guard (open transactions) — journal engine, Sprint 2 ──
    [Fact(Skip = "Blocked — AC#3 needs the journal/production-order engine (Sprint 2) that tags open " +
                 "transactions to a cost centre. HasOpenTransactionsAsync is a stub (false) until then; " +
                 "once real data exists, deactivating a cost centre with open txns must return 400 " +
                 "'open transactions in the current period'."),
     TestPriority(30)]
    public async Task CC030_Deactivate_Blocked_When_OpenTransactions()
    {
        await Task.CompletedTask;
    }
}
