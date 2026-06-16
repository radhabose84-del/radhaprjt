namespace SalesManagement.QATests.Tests.CustomerVisit;

// ─────────────────────────────────────────────────────────────────────────────
// CustomerVisit — live-server QA suite (TRANSACTIONAL entity).
//
// Contract verified against source (2026-06-15):
//   GET    /api/CustomerVisit?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/CustomerVisit/{id}                 (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/CustomerVisit/by-name?term=
//   POST   /api/CustomerVisit                      { customerId, visitTypeId, visitDateTime,
//                                                     marketingOfficerId, latitude?, longitude?,
//                                                     imageName?, remarks?, products:[{ itemId }] }
//   PUT    /api/CustomerVisit                      { id, customerId, visitTypeId, visitDateTime,
//                                                     marketingOfficerId, isActive, ... , products:[{ itemId }] }
//   DELETE /api/CustomerVisit?id={id}              (id bound from QUERY — simple int action param)
//   POST   /api/CustomerVisit/upload-image         ([FromForm] — multipart; not exercised here)
//   DELETE /api/CustomerVisit/delete-image?imagePath=
//
// Create command implements IRequirePermission (CanAdd) — auth is required and a
// permission claim must be present for the testsales user.
//
// Key facts that shaped assertions:
//   • CustomerId is a CROSS-MODULE FK (PartyManagement) validated via CustomerExistsAsync,
//     AND is further gated by a MarketingOfficerAccess filter (CanAccessCustomerAsync).
//   • VisitTypeId → Sales.MiscMaster (FKColumnDelete), MarketingOfficerId → same-module
//     /api/MarketingOfficer (MarketingOfficerExistsAsync).
//   • Create-happy is SKIPPED: it needs a Party the testsales marketing officer is
//     permitted to access plus a matching MarketingOfficer row — the QA clone has no
//     guaranteed satisfiable combination, and the access filter cannot be resolved from
//     read endpoints alone. Update/Delete lifecycle depend on a created row → also SKIPPED.
//   • All negatives, smoke (GetAll), and read-reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CustomerVisitCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CustomerVisitQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/CustomerVisit";

    public CustomerVisitQATests(QAServerFixture fixture) => _f = fixture;

    private static string VisitDate() => DateTimeOffset.UtcNow.ToString("o");

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy is BLOCKED; negatives are ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a Party the testsales marketing officer can access + a matching MarketingOfficer; MarketingOfficerAccess filter is not resolvable from read endpoints"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var customerId = await QAHelper.FirstIdAsync(_f.Client, "/api/Customer");
        var visitTypeId = await QAHelper.FirstIdAsync(_f.Client, "/api/sales/MiscMaster");
        var marketingOfficerId = await QAHelper.FirstIdAsync(_f.Client, "/api/MarketingOfficer");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId,
            visitTypeId,
            visitDateTime = VisitDate(),
            marketingOfficerId,
            remarks = "Created by QA suite",
            products = new[] { new { itemId = await QAHelper.FirstIdAsync(_f.Client, "/api/Item") } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 1,
            visitTypeId = 1,
            visitDateTime = VisitDate(),
            marketingOfficerId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_CustomerIdMissing_Returns400()
    {
        // CustomerId NotEmpty → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 0,
            visitTypeId = 1,
            visitDateTime = VisitDate(),
            marketingOfficerId = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_VisitTypeIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 1,
            visitTypeId = 0,
            visitDateTime = VisitDate(),
            marketingOfficerId = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_MarketingOfficerIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 1,
            visitTypeId = 1,
            visitDateTime = VisitDate(),
            marketingOfficerId = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_NonExistentFKs_Returns400()
    {
        // Non-existent customer/visitType/officer ids → FKColumnDelete + access filter fail.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 999999,
            visitTypeId = 999999,
            visitDateTime = VisitDate(),
            marketingOfficerId = 999999
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
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

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (no null guard → 200 + data:null when absent)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (lifecycle BLOCKED — needs a created row)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: depends on TC001 create which is blocked"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = 1,
            visitTypeId = 1,
            visitDateTime = VisitDate(),
            marketingOfficerId = 1,
            remarks = "Updated by QA",
            isActive = 1,
            products = new[] { new { itemId = 1 } }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            customerId = 1,
            visitTypeId = 1,
            visitDateTime = VisitDate(),
            marketingOfficerId = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact(Skip = "needs seeded data: depends on TC001 create which is blocked"), TestPriority(53)]
    public async Task TC053_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = 1,
            visitTypeId = 1,
            visitDateTime = VisitDate(),
            marketingOfficerId = 1,
            isActive = 0,
            products = new[] { new { itemId = 1 } }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns200Or400()
    {
        // Delete handler returns bool → controller wraps as 200 isSuccess:false for not-found,
        // unless the delete validator emits a 400. Tolerant to both.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact(Skip = "needs seeded data: depends on TC001 create which is blocked"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
