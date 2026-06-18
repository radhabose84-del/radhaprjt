namespace PurchaseManagement.QATests.Tests.OCREntry;

// ─────────────────────────────────────────────────────────────────────────────
// OCREntry — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — OCREntryController.cs).
//   ⚠ Route is derived from ApiControllerBase => [Route("api/[controller]")] => /api/OCREntry
//     (NOT /api/ocr — the controller class is OCREntryController; ASP.NET routes are
//      case-insensitive but the token is the full class name "OCREntry").
//   GET    /api/OCREntry?PageNumber=&PageSize=&SearchTerm=&StatusId=&FromDate=&ToDate=
//   GET    /api/OCREntry/{id}                                  (returns 200 + data:null when missing)
//   GET    /api/OCREntry/by-name?term=&approved=&showAll=
//   GET    /api/OCREntry/quality-template/{templateId}/parameters
//   GET    /api/OCREntry/pending?PageNumber=&PageSize=
//   POST   /api/OCREntry                                       CreateOCREntryCommand
//   PUT    /api/OCREntry                                       UpdateOCREntryCommand
//   DELETE /api/OCREntry?id={id}                               (id bound from QUERY)
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid OCR needs seeded procurement source/type + supplier/location/station/item plus
//   document-numbering series 'OCR' — none guaranteed on the QA clone. Reads / negatives /
//   reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("OCREntryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class OCREntryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/OCREntry";

    public OCREntryQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: procurement source/type + supplier/location/station/item + doc-numbering 'OCR'"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            ocrDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            procurementSourceId = 1,
            procurementTypeId = 1,
            supplierId = 1,
            locationId = 1,
            stationId = 1,
            ocrDetails = new[] { new { itemId = 1, quantity = 10m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { supplierId = 1 });
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
    public async Task TC030_AutoComplete_ByName_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetPending_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
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
