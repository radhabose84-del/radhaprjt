namespace SalesManagement.QATests.Tests.StoHeader;

// ─────────────────────────────────────────────────────────────────────────────
// StoHeader — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-15):
//   POST   /api/sales/StoHeader          { documentDate(DateOnly), expectedDeliveryDate(DateOnly>=documentDate),
//                                          stoTypeId, movementTypeId, supplyingPlantId, supplyingStorageLocationId,
//                                          receivingPlantId, receivingStorageLocationId, remarks?,
//                                          stoDetails:[{itemId, quantity, uomId, transferPrice}] }
//   PUT    /api/sales/StoHeader          { id, documentDate, expectedDeliveryDate, stoTypeId, movementTypeId,
//                                          supplyingPlantId, supplyingStorageLocationId, receivingPlantId,
//                                          receivingStorageLocationId, remarks?, isActive,
//                                          stoDetails:[{itemId, quantity, uomId, transferPrice}] }
//   DELETE /api/sales/StoHeader?id={id}   (id bound from QUERY — controller action param `int id`, default [FromQuery])
//   GET    /api/sales/StoHeader?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/sales/StoHeader/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/sales/StoHeader/pending?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/sales/StoHeader/pending/{id}
//   GET    /api/sales/StoHeader/by-name?term=
//
// ⚠️ Note the /api/sales/ ROUTE PREFIX (StoHeaderController = [Route("api/sales/[controller]")]).
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid StoHeader requires seeded supplying/receiving plant ids, storage-location ids, and at
//   least one inventory item id (with uom) — none of which the QA clone guarantees. These are
//   attribute-level [Fact(Skip=...)] so they are explicit pending work, not silent gaps.
//   Negatives (empty body / missing required / no-auth), smoke GetAll, AutoComplete, and the
//   pending/{id} read reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("StoHeaderCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class StoHeaderQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/sales/StoHeader";

    public StoHeaderQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: supplying/receiving plant ids, storage-location ids, and an inventory item+uom to build a valid StoHeader"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var documentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            documentDate = documentDate.ToString("yyyy-MM-dd"),
            expectedDeliveryDate = documentDate.AddDays(3).ToString("yyyy-MM-dd"),
            stoTypeId = 1,
            movementTypeId = 1,
            supplyingPlantId = 1,
            supplyingStorageLocationId = 1,
            receivingPlantId = 2,
            receivingStorageLocationId = 2,
            remarks = "Created by QA suite",
            stoDetails = new[]
            {
                new { itemId = 1, quantity = 10m, uomId = 1, transferPrice = 100m }
            }
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
            documentDate = "2026-01-01",
            expectedDeliveryDate = "2026-01-02",
            stoTypeId = 1,
            movementTypeId = 1
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
    public async Task TC004_Create_MissingRequiredFields_Returns400()
    {
        // Only a date supplied — FKs (stoType/movementType/plants/storage) and details missing.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            documentDate = "2026-01-01",
            expectedDeliveryDate = "2026-01-02"
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_ExpectedDeliveryBeforeDocumentDate_Returns400()
    {
        // expectedDeliveryDate must be >= documentDate.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            documentDate = "2026-06-10",
            expectedDeliveryDate = "2026-06-01",
            stoTypeId = 1,
            movementTypeId = 1,
            supplyingPlantId = 1,
            supplyingStorageLocationId = 1,
            receivingPlantId = 2,
            receivingStorageLocationId = 2,
            stoDetails = new[]
            {
                new { itemId = 1, quantity = 10m, uomId = 1, transferPrice = 100m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; tolerant 200/404)
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
    // SECTION 3 — EXTRA READS  (pending list + pending/{id} reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetPending_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetPendingById_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
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
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (lifecycle BLOCKED — depends on a created id; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created StoHeader id (TC001 is blocked on plant/storage/item seeds)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var documentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            documentDate = documentDate.ToString("yyyy-MM-dd"),
            expectedDeliveryDate = documentDate.AddDays(5).ToString("yyyy-MM-dd"),
            stoTypeId = 1,
            movementTypeId = 1,
            supplyingPlantId = 1,
            supplyingStorageLocationId = 1,
            receivingPlantId = 2,
            receivingStorageLocationId = 2,
            remarks = "Updated by QA",
            isActive = 1,
            stoDetails = new[]
            {
                new { itemId = 1, quantity = 12m, uomId = 1, transferPrice = 110m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            documentDate = "2026-01-01",
            expectedDeliveryDate = "2026-01-02",
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

    [Fact, TestPriority(53)]
    public async Task TC053_Update_MissingRequiredFields_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            documentDate = "2026-01-01",
            expectedDeliveryDate = "2026-01-02"
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (lifecycle BLOCKED; negatives ACTIVE — id bound from QUERY)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400Or404()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact(Skip = "needs seeded data: a created StoHeader id (TC001 is blocked on plant/storage/item seeds)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
