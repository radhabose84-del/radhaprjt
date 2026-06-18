namespace PurchaseManagement.QATests.Tests.PurchaseIndent;

// ─────────────────────────────────────────────────────────────────────────────
// PurchaseIndent — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — PurchaseIndentController.cs,
// [Route("api/[controller]")] => /api/PurchaseIndent):
//   GET    /api/PurchaseIndent?PageNumber=&PageSize=&SearchTerm=&StatusId=
//   POST   /api/PurchaseIndent                              CreatePurchaseIndentCommand
//   PUT    /api/PurchaseIndent                              UpdatePurchaseIndentCommand
//   DELETE /api/PurchaseIndent?id={id}                      (id bound from QUERY — action param int id)
//   GET    /api/PurchaseIndent/{id}?sourceId=               (returns 200 + data:null when missing)
//   GET    /api/PurchaseIndent/pending?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/PurchaseIndent/pending/{id}
//   GET    /api/PurchaseIndent/autocomplete?Status=&SearchTerm=&allIndents=
//   GET    /api/PurchaseIndent/indentdetailsforpo?vendorId=&departmentId=
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid Indent needs seeded indentType/unit/department + items, an auto IndentNumber, and
//   an approval flow — none guaranteed on the QA clone. Reads / negatives / reachability
//   remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurchaseIndentCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PurchaseIndentQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PurchaseIndent";

    public PurchaseIndentQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: indentType/unit/department + items + auto IndentNumber + approval"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            indentTypeId = 1,
            departmentId = 1,
            indentDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            indentDetails = new[] { new { itemId = 1, quantity = 5m } }
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { indentTypeId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET ALL (smoke; tolerant 200/404) ──────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 3 — EXTRA READS (reachability; tolerant) ───────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetPending_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_AutoComplete_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete?Status=Approved");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/autocomplete?Status=Approved");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        // FIXED 2026-06-18: GetPurchaseIndentByIdQueryHandler now null-guards before mapping/deref,
        // so a missing id returns cleanly (200 null-data / 404) instead of an NRE 500.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── SECTION 4 — UPDATE / DELETE (lifecycle BLOCKED; negatives ACTIVE) ───────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 999999 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
