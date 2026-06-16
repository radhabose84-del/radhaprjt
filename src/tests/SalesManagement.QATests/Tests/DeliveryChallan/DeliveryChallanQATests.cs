namespace SalesManagement.QATests.Tests.DeliveryChallan;

// ─────────────────────────────────────────────────────────────────────────────
// DeliveryChallan — live-server QA suite (TRANSACTIONAL entity). NO UPDATE endpoint.
//
// Contract verified against source (2026-06-15):
//   GET    /api/DeliveryChallan?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/DeliveryChallan/pending?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/DeliveryChallan/pending/{id}
//   GET    /api/DeliveryChallan/{id}/print
//   GET    /api/DeliveryChallan/{id}                 (200 + data:null when absent — no 404 guard)
//   GET    /api/DeliveryChallan/by-name?term=
//   GET    /api/DeliveryChallan/for-receipt?term=
//   GET    /api/DeliveryChallan/sto-open-qty?stoDetailId=
//   GET    /api/DeliveryChallan/gatepass-pending?vehicleNo=
//   POST   /api/DeliveryChallan                      { deliveryDate, stoHeaderId, fromPlantId,
//                                                       fromStorageLocationId, toPlantId,
//                                                       toStorageLocationId, transporterId,
//                                                       dcTypeId, movementTypeId, consignmentValue,
//                                                       details:[{ stoDetailId, itemId, lotId, ... }] }
//   POST   /api/DeliveryChallan/{id}/generate-ewaybill
//   GET    /api/DeliveryChallan/{id}/ewaybill-document
//   DELETE /api/DeliveryChallan?id={id}              (id bound from QUERY — simple int action param)
//
//   NO PUT/Update endpoint exists → no Update section.
//   Create command implements IRequirePermission (CanAdd).
//
// Key facts that shaped assertions:
//   • Create requires a seeded STO header + detail, plant/storage-location/transporter
//     references, and packed stock (lot/pack ranges in details). The QA clone has no
//     guaranteed seeded STO + stock → create-happy and delete-happy are SKIPPED.
//   • Reads (pending, print, by-name, for-receipt, sto-open-qty, gatepass-pending) are
//     reachability-tested (200/400/404 tolerant) — they may NRE/500 on empty data; the
//     contract is "endpoint exists & is auth-gated".
//   • All negatives + smoke (GetAll) remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DeliveryChallanCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DeliveryChallanQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DeliveryChallan";

    public DeliveryChallanQATests(QAServerFixture fixture) => _f = fixture;

    private static string Today() => DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a posted STO header + detail and packed stock (lot/pack ranges); QA clone has no guaranteed STO + stock"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            deliveryDate = Today(),
            stoHeaderId = 1,
            fromPlantId = 1,
            fromStorageLocationId = 1,
            toPlantId = 1,
            toStorageLocationId = 1,
            transporterId = 1,
            dcTypeId = 1,
            movementTypeId = 1,
            consignmentValue = 1000m,
            details = new[]
            {
                new { stoDetailId = 1, itemId = 1, lotId = 1, startPackNo = 1, endPackNo = 1 }
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
            deliveryDate = Today(),
            stoHeaderId = 1,
            dcTypeId = 1,
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
    public async Task TC004_Create_MissingRequired_Returns400()
    {
        // Missing stoHeaderId / dcTypeId / movementTypeId / details → required validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            deliveryDate = Today(),
            stoHeaderId = 0,
            dcTypeId = 0,
            movementTypeId = 0
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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID + EXTRA READS  (reachability)
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

    [Fact, TestPriority(32)]
    public async Task TC032_Pending_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_PendingById_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Print_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/print");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_ForReceipt_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/for-receipt?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_StoOpenQty_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/sto-open-qty?stoDetailId=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_GatePassPending_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/gatepass-pending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_EwaybillDocument_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/ewaybill-document");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(39)]
    public async Task TC039_Pending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=15");
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
    // SECTION 5 — GENERATE EWAYBILL  (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(60)]
    public async Task TC060_GenerateEwaybill_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/999999/generate-ewaybill", new { });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(61)]
    public async Task TC061_GenerateEwaybill_NonExistentDc_Returns200Or400Or404()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/999999/generate-ewaybill", new { });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (id bound from QUERY: ?id={id}; happy BLOCKED)
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
