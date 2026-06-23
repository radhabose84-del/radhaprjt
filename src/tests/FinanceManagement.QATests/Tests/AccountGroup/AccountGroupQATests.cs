namespace FinanceManagement.QATests.Tests.AccountGroup;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-02 — Account Group Hierarchy Builder (live-server QA).
//
// Unlike Schedule III, AccountGroup HAS a create endpoint, so the full lifecycle
// (create L1 → children → tree → move → update → delete) runs live. Needs only the
// seeded AccountTypeMaster (Asset = id 1) in the QA clone.
//
// Routes: api/finance/accountgroup
//   GET  /tree?companyId=     GET /{id}     GET /leaf-groups?companyId=&accountTypeId=
//   GET  /parents?level=      GET /by-name?term=
//   POST /  PUT /  POST /move  PUT /schedule-iii-mapping  DELETE ?id=
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AccountGroupCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AccountGroupQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/finance/accountgroup";
    private const int QACompanyId = 1;
    private const int AssetTypeId = 1;   // AccountTypeMaster "Asset" seeded in the QA clone

    // Cross-step ids (collection runs steps serially via PriorityOrderer).
    private static int _l1, _l2a, _l2b, _l3;

    public AccountGroupQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string Code(string suffix) => $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(8).ToArray())}{suffix}";

    // ── SMOKE ─────────────────────────────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetTree_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/tree?companyId={QACompanyId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ──────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetTree_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/tree?companyId={QACompanyId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute,
            new { companyId = QACompanyId, groupCode = "X", groupName = "X", accountTypeId = AssetTypeId, sortOrder = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Move_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/move",
            new { id = 1, newParentAccountGroupId = 2, justification = "x" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── VALIDATION (no seed needed) ───────────────────────────────────────────
    [Fact, TestPriority(5)]
    public async Task TC005_CreateL1_WithoutAccountType_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            new { companyId = QACompanyId, groupCode = Code("L1"), groupName = "No Type", accountTypeId = (int?)null, parentAccountGroupId = (int?)null, sortOrder = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("Account Type");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Move_MissingFields_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/move",
            new { id = 0, newParentAccountGroupId = 0, justification = "short" });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── FULL LIFECYCLE (live) ─────────────────────────────────────────────────
    [Fact, TestPriority(20)]
    public async Task TC020_Create_L1_And_Children_BuildsTree()
    {
        // L1 (needs accountTypeId, no parent)
        var r1 = await _f.Client.PostAsJsonAsync(BaseRoute,
            new { companyId = QACompanyId, groupCode = Code("A"), groupName = "QA Asset", accountTypeId = AssetTypeId, parentAccountGroupId = (int?)null, sortOrder = 1 });
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        _l1 = (await ParseAsync(r1)).RootElement.GetProperty("data").GetInt32();

        // child with accountTypeId set → rejected (AC: only L1 carries it)
        var rBad = await _f.Client.PostAsJsonAsync(BaseRoute,
            new { companyId = QACompanyId, groupCode = Code("BAD"), groupName = "Bad", accountTypeId = AssetTypeId, parentAccountGroupId = _l1, sortOrder = 1 });
        rBad.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // two L2 children + an L3 under L2a
        _l2a = await CreateChild(Code("CA"), "QA Current Assets", _l1);
        _l2b = await CreateChild(Code("NCA"), "QA Non-Current Assets", _l1);
        _l3 = await CreateChild(Code("INV"), "QA Inventories", _l2a);

        // tree reflects the nesting
        var tree = await _f.Client.GetAsync($"{BaseRoute}/tree?companyId={QACompanyId}");
        tree.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_DeleteParentWithChildren_Returns400()
    {
        _l2a.Should().BeGreaterThan(0);
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_l2a}");   // has child _l3
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_Move_L3_UnderL2b_SubmittedForApproval()
    {
        _l3.Should().BeGreaterThan(0);
        // circular / bad-level guard first: moving L3 under L1 (two levels up) is rejected
        var bad = await _f.Client.PostAsJsonAsync($"{BaseRoute}/move",
            new { id = _l3, newParentAccountGroupId = _l1, justification = "wrong level test long enough" });
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // valid: L2b is exactly one level above L3 → 200 "submitted" (deferred to multilevel approval)
        var ok = await _f.Client.PostAsJsonAsync($"{BaseRoute}/move",
            new { id = _l3, newParentAccountGroupId = _l2b, justification = "QA restructure for FY reporting" });
        ok.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(ok)).RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
    }

    [Fact, TestPriority(23)]
    public async Task TC023_Update_And_DeleteLeaf_CleanUp()
    {
        _l3.Should().BeGreaterThan(0);
        var upd = await _f.Client.PutAsJsonAsync(BaseRoute, new { id = _l3, groupName = "QA Inventories Updated", isActive = 1 });
        upd.StatusCode.Should().Be(HttpStatusCode.OK);

        // delete bottom-up (each becomes a leaf once emptied)
        (await _f.Client.DeleteAsync($"{BaseRoute}?id={_l3}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _f.Client.DeleteAsync($"{BaseRoute}?id={_l2a}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _f.Client.DeleteAsync($"{BaseRoute}?id={_l2b}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _f.Client.DeleteAsync($"{BaseRoute}?id={_l1}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── APPROVAL CHAIN (read-only Move-modal banner — configured multilevel chain FC → CFO) ──
    [Fact, TestPriority(24)]
    [Trait("Layer", "Smoke")]
    public async Task TC024_GetApprovalChain_HappyPath_Returns200WithLevelAndLabel()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/approval-chain");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = (await ParseAsync(resp)).RootElement;
        root.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
        data.GetArrayLength().Should().BeGreaterThan(0);   // config default falls back to FC → CFO
        foreach (var lvl in data.EnumerateArray())
        {
            lvl.TryGetProperty("level", out _).Should().BeTrue();
            lvl.TryGetProperty("label", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(25)]
    public async Task TC025_GetApprovalChain_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/approval-chain");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── MOVE PENDING (approval inbox — items awaiting the logged-in approver) ──
    [Fact, TestPriority(26)]
    [Trait("Layer", "Smoke")]
    public async Task TC026_GetMovePending_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/move-pending?pageNumber=1&pageSize=10");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var root = (await ParseAsync(resp)).RootElement;
        root.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);   // may be empty when nothing is pending for this user
        root.TryGetProperty("totalCount", out _).Should().BeTrue();   // API serializes camelCase
    }

    [Fact, TestPriority(27)]
    public async Task TC027_GetMovePending_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/move-pending");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<int> CreateChild(string code, string name, int parentId)
    {
        var r = await _f.Client.PostAsJsonAsync(BaseRoute,
            new { companyId = QACompanyId, groupCode = code, groupName = name, accountTypeId = (int?)null, parentAccountGroupId = parentId, sortOrder = 1 });
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await ParseAsync(r)).RootElement.GetProperty("data").GetInt32();
    }
}
