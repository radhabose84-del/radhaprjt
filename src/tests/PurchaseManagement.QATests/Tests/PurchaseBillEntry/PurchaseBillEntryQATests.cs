namespace PurchaseManagement.QATests.Tests.PurchaseBillEntry;

// ─────────────────────────────────────────────────────────────────────────────
// PurchaseBillEntry — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — PurchaseOrder/PurchaseBillEntryController.cs,
// [Route("api/[controller]")] => /api/PurchaseBillEntry):
//   POST   /api/PurchaseBillEntry                       PurchaseBillEntryHeaderDto body
//   PUT    /api/PurchaseBillEntry/{id:int}              (route id must match body.Id)
//   GET    /api/PurchaseBillEntry/{id:int}
//   GET    /api/PurchaseBillEntry?vendorId=&search=&fromDate=&toDate=&page=&size=
//          (⚠ paging params are `page`/`size` — NOT PageNumber/PageSize)
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid bill needs a seeded party + PO/GRN plus bill lines — none guaranteed on the QA
//   clone. Reads / negatives remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurchaseBillEntryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PurchaseBillEntryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PurchaseBillEntry";

    public PurchaseBillEntryQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: party + PO/GRN + bill lines"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            vendorId = 1,
            billDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            details = new[] { new { itemId = 1, quantity = 5m, rate = 100m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { vendorId = 1 });
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?page=1&size=15");
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?page=1&size=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_FilterByVendor_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?vendorId=999999&page=1&size=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 3 — GET BY ID (reachability; tolerant) ─────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 4 — UPDATE (lifecycle BLOCKED; negatives ACTIVE) ────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/999999", new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/999999", new { id = 999999 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
