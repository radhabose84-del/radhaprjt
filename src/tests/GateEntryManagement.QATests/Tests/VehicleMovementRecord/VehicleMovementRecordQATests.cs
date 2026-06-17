namespace GateEntryManagement.QATests.Tests.VehicleMovementRecord;

// ─────────────────────────────────────────────────────────────────────────────
// VehicleMovementRecord (GateEntry) — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-16):
//   POST   /api/gateentry/VehicleMovementRecord  { vehicleNumber?, driverName?, driverLicenseNo?,
//                                                   driverMobileNo?(10-digit), transporterId?,
//                                                   purposeOfVisitId(REQUIRED, MiscMaster FK),
//                                                   referenceDocTypeId?, referenceDocNo?,
//                                                   unitId(REQUIRED), remarks? }
//   PUT    /api/gateentry/VehicleMovementRecord  { id, ...same fields..., isActive }
//   DELETE /api/gateentry/VehicleMovementRecord?id={id}  (id bound from QUERY, not route)
//   GET    /api/gateentry/VehicleMovementRecord?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/gateentry/VehicleMovementRecord/{id}      (200 + data:null when not found — NO 404 guard)
//   GET    /api/gateentry/VehicleMovementRecord/by-name?term=&purposeOfVisitId=
//   GET    /api/gateentry/VehicleMovementRecord/pending?VehicleNumber=
//
// Why create-happy + lifecycle are SKIPPED:
//   Create auto-generates VehicleMovementId via the doc-numbering engine (TransactionType
//   "Gate Entry" + DocumentSequence) which the QA clone does not seed for the GateEntry module,
//   and additionally requires valid unitId + purposeOfVisitId (MiscMaster) seeds. These are
//   attribute-level [Fact(Skip=...)] — explicit pending work, not silent gaps. Negatives
//   (empty body / missing required / no-auth), smoke GetAll, by-name + pending reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("VehicleMovementRecordCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class VehicleMovementRecordQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/gateentry/VehicleMovementRecord";

    public VehicleMovementRecordQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: GateEntry document-numbering (TransactionType 'Gate Entry' + DocumentSequence) not seeded for GateEntry module + unitId/purposeOfVisitId"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var purposeOfVisitId = await QAHelper.FirstIdAsync(_f.Client, "/api/gateentry/MiscMaster");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            vehicleNumber = "QA" + _f.EntityCode[..6],
            driverName = "QA Driver",
            driverLicenseNo = "DL" + _f.EntityCode[..6],
            driverMobileNo = "9876543210",
            purposeOfVisitId = purposeOfVisitId > 0 ? purposeOfVisitId : 1,
            unitId = 1,
            remarks = "Created by QA suite"
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
            vehicleNumber = "QA0001",
            purposeOfVisitId = 1,
            unitId = 1
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
        // purposeOfVisitId + unitId are required → default 0 fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            vehicleNumber = "QA0001",
            driverName = "QA Driver"
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
    // SECTION 3 — EXTRA READS  (GetById, by-name, pending reachability)
    // ─────────────────────────────────────────────────────────────────────────

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

    [Fact, TestPriority(32)]
    public async Task TC032_GetPending_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?VehicleNumber=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?VehicleNumber=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (params: term, optional purposeOfVisitId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
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

    [Fact(Skip = "needs seeded data: a created VehicleMovementRecord id (TC001 is blocked on GateEntry doc-numbering + unit/purpose seeds)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            vehicleNumber = "QA0001",
            driverName = "QA Driver Updated",
            driverMobileNo = "9876543210",
            purposeOfVisitId = 1,
            unitId = 1,
            remarks = "Updated by QA",
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            purposeOfVisitId = 1,
            unitId = 1,
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

    [Fact(Skip = "needs seeded data: a created VehicleMovementRecord id (TC001 is blocked on GateEntry doc-numbering + unit/purpose seeds)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
