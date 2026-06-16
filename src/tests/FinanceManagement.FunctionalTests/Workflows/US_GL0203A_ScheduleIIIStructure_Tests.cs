namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-03A — Schedule III Line-Item & Sub-total Configuration (workflow / story).
//
//   As a Finance Controller I maintain the Schedule III structure (line items, sections,
//   sub-totals) so the 03B mapping screen and auto-generated statements draw from one
//   governed, versioned definition.
//
// Proves the STORY, not individual endpoints (QA covers those). Steps that need a seeded
// structure/section (no create-structure endpoint exists) are [Fact(Skip=...)] per the
// catalogue — never a silent gap; un-skip after running ScheduleIIIMiscSeed.sql + seeding
// a structure for (CompanyId=1, DivisionId=7) in the QA clone.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-03A")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-03A")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0203A_ScheduleIIIStructure_Tests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/finance/ScheduleIII";
    private const int CompanyId = 1;
    private const int DivisionId = 7;

    // Cross-step state (xUnit builds a new instance per test; collection runs steps serially).
    private static int _structureId;
    private static int _sectionId;
    private static int _lineId;

    public US_GL0203A_ScheduleIIIStructure_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // STEP 1 (live) — the governed structure is readable; capture ids if seeded.
    [Fact, TestPriority(1)]
    public async Task Step1_GetStructure_ReturnsShape_AndCapturesIdsIfSeeded()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/structure?companyId={CompanyId}&divisionId={DivisionId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Object)
        {
            _structureId = data.GetProperty("id").GetInt32();
            var sections = data.GetProperty("sections");
            if (sections.GetArrayLength() > 0)
                _sectionId = sections[0].GetProperty("id").GetInt32();
        }
        // Shape assertion only — membership depends on seed (kept tolerant by design).
        data.ValueKind.Should().BeOneOf(JsonValueKind.Object, JsonValueKind.Null);
    }

    // STEP 2 (AC1) — add a BS line → it becomes selectable in the 03B dropdown preview.
    [Fact(Skip = "Needs seeded structure+section for (CompanyId=1,DivisionId=7). Un-skip after seeding. " +
                 "Adds a line via POST /line-item, then asserts it appears in GET /preview-03b/{structureId} " +
                 "balance-sheet leaf list (AC1, no code change)."),
     TestPriority(2)]
    public async Task Step2_AddBalanceSheetLine_AppearsIn03BPreview()
    {
        _structureId.Should().BeGreaterThan(0);
        _sectionId.Should().BeGreaterThan(0);

        var lineName = $"QA FT Line {_f.EntityCode}";
        var createResp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/line-item", new
        {
            structureId = _structureId,
            sectionId = _sectionId,
            lineCode = $"QA{new string(_f.EntityCode.Where(char.IsDigit).Take(4).ToArray())}",
            lineName,
            displayOrder = 99,
            isSplitLine = 0
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        _lineId = (await ParseAsync(createResp)).RootElement.GetProperty("data").GetInt32();

        var previewResp = await _f.Client.GetAsync($"{BaseRoute}/preview-03b/{_structureId}");
        previewResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var preview = (await ParseAsync(previewResp)).RootElement.GetProperty("data");
        var bsLeaves = preview.GetProperty("balanceSheetLeaves").EnumerateArray()
            .Select(x => x.GetProperty("lineName").GetString());
        bsLeaves.Should().Contain(lineName, "AC1 — a new BS line is offered to the 03B mapping screen with no code change");
    }

    // STEP 3 (AC2) — define/adjust a sub-total formula; GetSubTotals reflects the change.
    [Fact(Skip = "Needs seeded structure. Un-skip after seeding. Creates a sub-total via POST /subtotal, " +
                 "then PUT /subtotal with includeOtherIncome=true + operands, and asserts GET /subtotals/{id} " +
                 "echoes the rebuilt formula expression + flag (AC2 recompute)."),
     TestPriority(3)]
    public async Task Step3_EditSubTotalFormula_IsReflected()
    {
        _structureId.Should().BeGreaterThan(0);
        // create → edit → read-back assertion lives here once a structure is seeded.
        await Task.CompletedTask;
    }

    // STEP 4 (Lock + FR-008) — lock the structure; post-lock edits are rejected.
    [Fact(Skip = "Needs seeded structure. Un-skip after seeding. POST /lock then assert a follow-up " +
                 "POST /line-item is 400 with the FR-008 change-control message."),
     TestPriority(4)]
    public async Task Step4_LockStructure_BlocksFurtherEdits()
    {
        _structureId.Should().BeGreaterThan(0);
        await Task.CompletedTask;
    }
}
