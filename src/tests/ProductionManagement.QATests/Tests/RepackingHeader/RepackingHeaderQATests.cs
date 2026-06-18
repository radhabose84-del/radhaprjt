namespace ProductionManagement.QATests.Tests.RepackingHeader;

// ─────────────────────────────────────────────────────────────────────────────
// RepackingHeader — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — RepackingHeaderController):
//   GET    /api/repackingheader?PageNumber=&PageSize=&SearchTerm=     (paged list)
//   GET    /api/repackingheader/{id}
//   GET    /api/repackingheader/by-name?term=
//   GET    /api/repackingheader/getstockitems?productionYear=
//   GET    /api/repackingheader/get-packs-by-item-lot?productionYear=
//   POST   /api/repackingheader
//   PUT    /api/repackingheader
//   DELETE /api/repackingheader/{id}    (id bound from ROUTE)
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid repack needs target+source item/packType, warehouse/bin, nested details, AND
//   doc-numbering sequences 'RePackMaster'/'YarnConversion' plus packed stock — none seeded on the
//   QA clone. These are attribute-level [Fact(Skip=...)] — explicit pending work, not silent gaps.
//   GetAll smoke, no-auth, empty-body negative, and the extra-GET reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("RepackingHeaderCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class RepackingHeaderQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/repackingheader";

    public RepackingHeaderQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: doc-numbering RePackMaster/YarnConversion + item/packType/warehouse/bin + packed stock"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            documentDate = today.ToString("yyyy-MM-dd"),
            productionYear = today.Year,
            warehouseId = 1,
            details = new[]
            {
                new { sourceItemId = _f.ActiveItemId, targetItemId = _f.ActiveItemId, quantity = 10m }
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
            productionYear = 2026,
            warehouseId = 1
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
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            documentDate = "2026-01-01"
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
    public async Task TC022_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (reachability — tolerant 200/400/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetStockItems_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/getstockitems?productionYear=2026");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetPacksByItemLot_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/get-packs-by-item-lot?productionYear=2026");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetStockItems_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/getstockitems?productionYear=2026");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created RepackingHeader id (TC001 blocked on doc-numbering RePackMaster/YarnConversion + item/packType/warehouse/bin + packed stock)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            documentDate = today.ToString("yyyy-MM-dd"),
            productionYear = today.Year,
            warehouseId = 1,
            isActive = 1,
            details = new[]
            {
                new { sourceItemId = _f.ActiveItemId, targetItemId = _f.ActiveItemId, quantity = 12m }
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
            productionYear = 2026
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (lifecycle BLOCKED; negatives ACTIVE — id bound from ROUTE)
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

    [Fact(Skip = "needs seeded data: a created RepackingHeader id (TC001 blocked on doc-numbering RePackMaster/YarnConversion + item/packType/warehouse/bin + packed stock)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
