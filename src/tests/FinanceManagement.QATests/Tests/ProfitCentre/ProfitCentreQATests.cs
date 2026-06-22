namespace FinanceManagement.QATests.Tests.ProfitCentre;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL05-02 — Profit Centre master & 2-level hierarchy (live-server QA).
//
// Route: api/finance/ProfitCentre
//   GET (list, paged+search) · GET {id} · GET by-name?term=&level= (autocomplete / parent picker)
//   POST · PUT · DELETE ?id=   (CompanyId from the token; ProfitCentreCode immutable, unique ACROSS
//   all companies; hyphens allowed (PC-SPIN-001); LevelId → Finance.MiscMaster 'PROFITCENTRELEVEL')
//
// Level ids differ per environment → resolved at runtime from miscmaster/by-name?MiscTypeCode=PROFITCENTRELEVEL.
// The L1 (Segment) lifecycle is self-contained (no cross-module FK) and runs live. The L2 sub-segment step
// chains onto a created L1.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ProfitCentreCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ProfitCentreQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/ProfitCentre";

    public ProfitCentreQATests(QAServerFixture fixture) => _f = fixture;

    // Code: alphanumeric + hyphens, <= 20 chars, unique across all companies.
    private string UniqueCode(string suffix = "") =>
        $"PCQA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(12).ToArray())}{suffix}";

    private static string? TryStr(JsonElement e, string p) =>
        e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static int TryInt(JsonElement e, string p) =>
        e.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    // Resolve a PROFITCENTRELEVEL misc id by its code (PCL1 / PCL2). 0 if not seeded.
    private async Task<int> ResolveLevelIdAsync(string levelCode)
    {
        var resp = await _f.Client.GetAsync("/api/finance/miscmaster/by-name?MiscTypeCode=PROFITCENTRELEVEL");
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

    // ── SMOKE — login → auth → DB → read works (deploy gate) ──────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task PC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await QAHelper.ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task PC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task PC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{Route}", new { profitCentreCode = UniqueCode(), levelId = 1 });
        await QAHelper.Assert401Async(resp);
    }

    // ── VALIDATION negatives ──────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task PC004_Create_MissingRequired_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{Route}", new { profitCentreCode = "", profitCentreName = "", levelId = 0 });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task PC005_Create_InvalidCharCode_Returns400()
    {
        // Spaces and special chars are rejected; hyphens are allowed (covered by the lifecycle below).
        var resp = await _f.Client.PostAsJsonAsync($"{Route}",
            new { profitCentreCode = "PC@01", profitCentreName = "Bad Code", levelId = 1, parentProfitCentreId = (int?)null });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task PC006_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{Route}", new { id = 0, profitCentreName = "", isActive = 1 });
        await QAHelper.Assert400Async(resp);
    }

    // ── FULL L1 LIFECYCLE (self-contained; only needs the PROFITCENTRELEVEL L1 id) ──
    [Fact, TestPriority(10)]
    public async Task PC010_FullL1Lifecycle()
    {
        var l1Level = await ResolveLevelIdAsync("PCL1");
        if (l1Level == 0) return;    // self-skip: PROFITCENTRELEVEL not seeded in this clone

        var code = UniqueCode();     // contains no hyphen; hyphen acceptance covered by PC020 child code

        // Create L1 Segment
        var createResp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            profitCentreCode = code,
            profitCentreName = $"QA Segment {_f.EntityCode}",
            levelId = l1Level,
            parentProfitCentreId = (int?)null,
            responsibleHeadId = (int?)null,
            isRevenueLinked = true
        });
        await QAHelper.AssertOkAsync(createResp);
        var id = await QAHelper.GetCreatedIdAsync(createResp);
        id.Should().BeGreaterThan(0);

        // Duplicate code (global uniqueness) → 400
        var dupResp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            profitCentreCode = code,
            profitCentreName = $"QA Segment Dup {_f.EntityCode}",
            levelId = l1Level,
            parentProfitCentreId = (int?)null
        });
        await QAHelper.Assert400Async(dupResp);

        // GET by id
        (await _f.Client.GetAsync($"{Route}/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        // by-name autocomplete (filtered to L1) contains it
        var byNameResp = await _f.Client.GetAsync($"{Route}/by-name?term={code}&level={l1Level}");
        byNameResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await QAHelper.ParseAsync(byNameResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("profitCentreCode").GetString());
        codes.Should().Contain(code);

        // Update (rename)
        var updResp = await _f.Client.PutAsJsonAsync($"{Route}",
            new { id, profitCentreName = $"QA Segment Edited {_f.EntityCode}", isActive = 1, isRevenueLinked = true });
        await QAHelper.AssertOkAsync(updResp);

        // Deactivate → hidden from autocomplete, kept in GetAll
        var deactResp = await _f.Client.PutAsJsonAsync($"{Route}",
            new { id, profitCentreName = $"QA Segment Edited {_f.EntityCode}", isActive = 0, isRevenueLinked = true });
        await QAHelper.AssertOkAsync(deactResp);

        var byName2 = await _f.Client.GetAsync($"{Route}/by-name?term={code}&level={l1Level}");
        var codes2 = (await QAHelper.ParseAsync(byName2)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("profitCentreCode").GetString());
        codes2.Should().NotContain(code);

        // Delete (soft) — no children/current-year-txns, succeeds
        var delResp = await _f.Client.DeleteAsync($"{Route}?id={id}");
        await QAHelper.AssertOkAsync(delResp);
    }

    // ── HIERARCHY — L2 Sub-segment under an L1 Segment; hyphenated code accepted ──
    [Fact, TestPriority(20)]
    public async Task PC020_L2_Under_L1_RollsUpToSegment()
    {
        var l1Level = await ResolveLevelIdAsync("PCL1");
        var l2Level = await ResolveLevelIdAsync("PCL2");
        if (l1Level == 0 || l2Level == 0) return;   // self-skip: levels not seeded

        // Create the L1 parent
        var l1Resp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            profitCentreCode = UniqueCode("S"),
            profitCentreName = $"QA Segment {_f.EntityCode}",
            levelId = l1Level,
            parentProfitCentreId = (int?)null
        });
        await QAHelper.AssertOkAsync(l1Resp);
        var l1Id = await QAHelper.GetCreatedIdAsync(l1Resp);

        // Create the hyphenated L2 under it (AC#4 mid-year justification supplied)
        var l2Resp = await _f.Client.PostAsJsonAsync($"{Route}", new
        {
            profitCentreCode = $"{UniqueCode("S")}-01",   // hyphen — allowed for PC codes
            profitCentreName = $"QA Sub-segment {_f.EntityCode}",
            levelId = l2Level,
            parentProfitCentreId = l1Id,
            midYearJustification = "QA mid-year addition"
        });
        await QAHelper.AssertOkAsync(l2Resp);
        var l2Id = await QAHelper.GetCreatedIdAsync(l2Resp);

        // L2 row rolls up to its L1 parent name (AC#3 consolidation)
        var byId = await _f.Client.GetAsync($"{Route}/{l2Id}");
        var data = (await QAHelper.ParseAsync(byId)).RootElement.GetProperty("data");
        data.GetProperty("parentProfitCentreId").GetInt32().Should().Be(l1Id);
        data.GetProperty("parentProfitCentreName").GetString().Should().NotBeNullOrEmpty();

        // Deleting the parent while a child exists is blocked → 400
        var delParent = await _f.Client.DeleteAsync($"{Route}?id={l1Id}");
        await QAHelper.Assert400Async(delParent);

        // cleanup leaf-first
        await _f.Client.DeleteAsync($"{Route}?id={l2Id}");
        await _f.Client.DeleteAsync($"{Route}?id={l1Id}");
    }

    // ── AC#5 deactivation guard (current-year transactions) — journal engine, Sprint 2 ──
    [Fact(Skip = "Blocked — AC#5 needs the GL journal engine (Sprint 2) that tags transactions to a " +
                 "profit centre. HasCurrentYearTransactionsAsync is a stub (false) until then; once real " +
                 "data exists, deactivating a PC with current-FY transactions must return 400 " +
                 "'Deactivation is blocked until year-end close'."),
     TestPriority(30)]
    public async Task PC030_Deactivate_Blocked_When_CurrentYearTransactions()
    {
        await Task.CompletedTask;
    }
}
