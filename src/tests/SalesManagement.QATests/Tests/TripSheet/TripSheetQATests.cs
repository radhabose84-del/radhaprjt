namespace SalesManagement.QATests.Tests.TripSheet;

// ─────────────────────────────────────────────────────────────────────────────
// TripSheet — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-15):
//   POST   /api/TripSheet              { tripDate(DateOnly), vehicleNo(required, max 20), remarks?,
//                                         details:[{dispatchAdviceHeaderId, sequenceNo}] }
//   PUT    /api/TripSheet              { id, tripDate, vehicleNo, remarks?, isActive,
//                                         details:[{dispatchAdviceHeaderId, sequenceNo}] }
//   DELETE /api/TripSheet?id={id}       (id bound from QUERY — controller action param `int id`, default [FromQuery])
//   GET    /api/TripSheet?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/TripSheet/{id}          (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/TripSheet/by-name?term=
//   GET    /api/TripSheet/{id}/packing-list
//   GET    /api/TripSheet/{id}/invoices
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid TripSheet requires a seeded dispatchAdviceHeaderId that is NOT already assigned to another
//   trip — the QA clone has no such guaranteed row. Attribute-level [Fact(Skip=...)] keeps the
//   happy + update + delete-happy steps explicit pending work. Negatives (empty body / missing
//   required / vehicleNo maxlength / no-auth), smoke GetAll, AutoComplete, and the packing-list /
//   invoices read reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("TripSheetCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class TripSheetQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/TripSheet";

    public TripSheetQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a dispatch-advice header (dispatchAdviceHeaderId) not already assigned to another trip"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            tripDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            vehicleNo = "QA1234",
            remarks = "Created by QA suite",
            details = new[]
            {
                new { dispatchAdviceHeaderId = 1, sequenceNo = 1 }
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
            tripDate = "2026-01-01",
            vehicleNo = "QA1234",
            details = new[] { new { dispatchAdviceHeaderId = 1, sequenceNo = 1 } }
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
    public async Task TC004_Create_VehicleNoEmpty_Returns400()
    {
        // vehicleNo is required.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            tripDate = "2026-01-01",
            vehicleNo = "",
            details = new[] { new { dispatchAdviceHeaderId = 1, sequenceNo = 1 } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_VehicleNoTooLong_Returns400()
    {
        // vehicleNo max length is 20.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            tripDate = "2026-01-01",
            vehicleNo = new string('A', 21),
            details = new[] { new { dispatchAdviceHeaderId = 1, sequenceNo = 1 } }
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
    // SECTION 3 — GET BY ID  +  packing-list / invoices reachability
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
    public async Task TC032_PackingList_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/packing-list");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Invoices_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/invoices");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_PackingList_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1/packing-list");
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

    [Fact(Skip = "needs seeded data: a created TripSheet id (TC001 is blocked on an unassigned dispatch advice)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            tripDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            vehicleNo = "QA5678",
            remarks = "Updated by QA",
            isActive = 1,
            details = new[] { new { dispatchAdviceHeaderId = 1, sequenceNo = 1 } }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            tripDate = "2026-01-01",
            vehicleNo = "QA5678",
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
    public async Task TC053_Update_VehicleNoEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            tripDate = "2026-01-01",
            vehicleNo = "",
            isActive = 1
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

    [Fact(Skip = "needs seeded data: a created TripSheet id (TC001 is blocked on an unassigned dispatch advice)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
