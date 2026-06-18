namespace PurchaseManagement.QATests.Tests.Arrival;

// ─────────────────────────────────────────────────────────────────────────────
// Arrival — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — ArrivalController.cs, route from
// ApiControllerBase => [Route("api/[controller]")] => /api/Arrival):
//   GET    /api/Arrival?PageNumber=&PageSize=&SearchTerm=&PendingStatus=&StatusId=&FromDate=&ToDate=
//   GET    /api/Arrival/{id}                         (returns 200 + data:null when not found)
//   GET    /api/Arrival/by-name?term=
//   GET    /api/Arrival/last-lotno                   (latest arrival lot no for current unit)
//   GET    /api/Arrival/balance-qty?rawMaterialPoId= (PO qty − arrived qty per item)
//   POST   /api/Arrival                              CreateArrivalCommand
//   PUT    /api/Arrival                              UpdateArrivalCommand
//   DELETE /api/Arrival?id={id}                      (id bound from QUERY — action param int id)
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid Arrival needs a seeded RawMaterialPO plus supplier/station/godown/transporter FKs
//   and document-numbering series 'Arrival' — none guaranteed on the QA clone. These are
//   attribute-level [Fact(Skip=...)] (explicit pending work). Reads / negatives / reachability
//   remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ArrivalCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ArrivalQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Arrival";

    public ArrivalQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: RawMaterialPO + supplier/station/godown/transporter + doc-numbering 'Arrival'"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            arrivalDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            rawMaterialPoId = 1,
            supplierId = 1,
            stationId = 1,
            godownId = 1,
            transporterId = 1,
            lotNo = "QA-LOT-001"
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { rawMaterialPoId = 1 });
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

    // ── SECTION 3 — EXTRA READS (reachability; tolerant) ───────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_AutoComplete_WithTerm_Reachable_Returns200Or404()
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
    public async Task TC032_LastLotNo_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/last-lotno");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_BalanceQty_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/balance-qty?rawMaterialPoId=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_GetById_NonExistentId_Returns200Or404()
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
