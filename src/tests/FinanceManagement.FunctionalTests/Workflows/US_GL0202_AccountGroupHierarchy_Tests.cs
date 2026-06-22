namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-02 — Account Group Hierarchy Builder (workflow / story).
//
//   As a Finance Controller I maintain the Segment → Group → Sub-group → Account
//   hierarchy so balances roll up at every level and accounts attach only at the leaf.
//
// Proves the STORY end-to-end against the live server (QA covers per-endpoint contract).
// Self-contained steps run live (create tree → AC1 leaf shape → AC3 circular/level guard →
// Move submitted for approval). Steps that need extra seed/infra are [Fact(Skip=...)] with
// a clear reason — never a silent gap:
//   • AC2 leaf-only GL assign needs GlAccountMaster lookups (NormalBalance/Currency/SubLedger).
//   • Move "applied after approval" needs RabbitMQ + Worker (covered by ApprovedRejectedConsumer
//     unit tests + manual E2E verification 2026-06-16).
//   • FR-003 Schedule III map needs a seeded ScheduleIIILineItem.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-02")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0202_AccountGroupHierarchy_Tests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/finance/accountgroup";
    private const int CompanyId = 1;
    private const int AssetTypeId = 1;   // AccountTypeMaster "Asset"

    private static int _l1, _l2a, _l2b, _l3;

    public US_GL0202_AccountGroupHierarchy_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string Code(string s) => $"FT{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(8).ToArray())}{s}";

    private async Task<int> Create(string code, string name, int? accountTypeId, int? parentId)
    {
        var r = await _f.Client.PostAsJsonAsync(BaseRoute,
            new { companyId = CompanyId, groupCode = code, groupName = name, accountTypeId, parentAccountGroupId = parentId, sortOrder = 1 });
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await ParseAsync(r)).RootElement.GetProperty("data").GetInt32();
    }

    // STEP 1 (live) — the hierarchy is readable.
    [Fact, TestPriority(1)]
    public async Task Step1_GetTree_ReturnsShape()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/tree?companyId={CompanyId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    // STEP 2 (AC1) — build Segment→Group→Sub-group; the leaf is the only IsLeaf node.
    [Fact, TestPriority(2)]
    public async Task Step2_BuildBranch_LeafOnlyAtBottom()
    {
        _l1 = await Create(Code("A"), "FT Asset", AssetTypeId, null);
        _l2a = await Create(Code("CA"), "FT Current Assets", null, _l1);
        _l2b = await Create(Code("NCA"), "FT Non-Current Assets", null, _l1);
        _l3 = await Create(Code("INV"), "FT Inventories", null, _l2a);

        var detail = (await ParseAsync(await _f.Client.GetAsync($"{BaseRoute}/{_l3}"))).RootElement.GetProperty("data");
        detail.GetProperty("level").GetInt32().Should().Be(3, "AC1 — derived depth Segment→Group→Sub-group");
        detail.GetProperty("isLeaf").GetBoolean().Should().BeTrue("AC1 — accounts attach only at the leaf");

        var parent = (await ParseAsync(await _f.Client.GetAsync($"{BaseRoute}/{_l2a}"))).RootElement.GetProperty("data");
        parent.GetProperty("isLeaf").GetBoolean().Should().BeFalse("a node with a child is not a leaf");
    }

    // STEP 3 (AC1) — the leaf appears in the assignable leaf-groups picker for its head.
    [Fact, TestPriority(3)]
    public async Task Step3_Leaf_AppearsInLeafGroups()
    {
        _l3.Should().BeGreaterThan(0);
        var resp = await _f.Client.GetAsync($"{BaseRoute}/leaf-groups?companyId={CompanyId}&accountTypeId={AssetTypeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var ids = (await ParseAsync(resp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("id").GetInt32());
        ids.Should().Contain(_l3, "the new leaf under the Asset head is offered to the GL-account picker");
    }

    // STEP 4 (AC3) — circular / wrong-level move is blocked.
    [Fact, TestPriority(4)]
    public async Task Step4_BadMove_Rejected()
    {
        _l3.Should().BeGreaterThan(0);
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/move",
            new { id = _l3, newParentAccountGroupId = _l1, justification = "wrong level functional test" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest, "AC3 — new parent must be exactly one level above");
    }

    // STEP 5 (Move) — valid move is accepted and deferred for Finance Controller approval.
    [Fact, TestPriority(5)]
    public async Task Step5_ValidMove_SubmittedForApproval()
    {
        _l3.Should().BeGreaterThan(0);
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/move",
            new { id = _l3, newParentAccountGroupId = _l2b, justification = "FT restructure for FY reporting" });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await ParseAsync(resp)).RootElement;
        body.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        body.GetProperty("message").GetString().Should().Contain("approval", "move is gated — not applied until approved");
    }

    // STEP 6 (AC2) — accounts attach only at the leaf (needs GlAccountMaster lookups seeded).
    [Fact(Skip = "Needs GlAccountMaster reference lookups (NormalBalance/Currency/SubLedger MiscMaster) in the QA clone. " +
                 "Un-skip to assert: POST /api/finance/glaccountmaster with accountGroupId=<leaf> → 200, and with " +
                 "accountGroupId=<non-leaf> → 400 'Accounts attach only at leaf level' (AC2)."),
     TestPriority(6)]
    public async Task Step6_GlAccount_AttachesOnlyAtLeaf() => await Task.CompletedTask;

    // STEP 7 (FR-003) — map the sub-group to a Schedule III line (needs a seeded line item).
    [Fact(Skip = "Needs a seeded Finance.ScheduleIIILineItem in the QA clone. Un-skip to assert: " +
                 "PUT /api/finance/accountgroup/schedule-iii-mapping {accountGroupId, scheduleIIILineItemId} → 200, " +
                 "then GET /{id} shows scheduleIIILineName (FR-003)."),
     TestPriority(7)]
    public async Task Step7_ScheduleIII_Mapping() => await Task.CompletedTask;

    // STEP 8 (cleanup) — soft-delete the branch bottom-up.
    [Fact, TestPriority(8)]
    public async Task Step8_CleanUp()
    {
        if (_l3 > 0) await _f.Client.DeleteAsync($"{BaseRoute}?id={_l3}");
        if (_l2a > 0) await _f.Client.DeleteAsync($"{BaseRoute}?id={_l2a}");
        if (_l2b > 0) await _f.Client.DeleteAsync($"{BaseRoute}?id={_l2b}");
        if (_l1 > 0) await _f.Client.DeleteAsync($"{BaseRoute}?id={_l1}");
    }
}
