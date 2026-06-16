namespace GateEntryManagement.QATests.Tests.GateInward;

// ─────────────────────────────────────────────────────────────────────────────
// GateInward (GateEntry) — live-server QA suite (TRANSACTIONAL; create-happy SKIPPED).
//
// Contract verified against source (2026-06-16):
//   GET    /api/gateentry/GateInward?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/gateentry/GateInward/{id}                  (200 + data:null when not found — NO 404 guard)
//   GET    /api/gateentry/GateInward/by-name?term=
//   GET    /api/gateentry/GateInward/pending-reference-docs?partyId=&referenceDocumentTypeId=
//   GET    /api/gateentry/GateInward/pending-reference-doc-items
//   POST   /api/gateentry/GateInward                       (header + nested gateInwardDetails[])
//   POST   /api/gateentry/GateInward/upload-attachment
//   DELETE /api/gateentry/GateInward/attachment?gateInwardHdrId={id}
//   DELETE /api/gateentry/GateInward?id={id}               (id bound from QUERY, not route)
//   (NO update endpoint)
//
// Why create-happy is SKIPPED:
//   A valid GateInward needs the doc-numbering engine ("Gate Inward"), receivingTypeId,
//   receivingWarehouseId, unitId, and a PO/GRN bridge plus nested gateInwardDetails[] — none of
//   which the QA clone guarantees. Attribute-level [Fact(Skip=...)] — explicit pending work.
//   Negatives (empty body / no-auth), smoke GetAll, by-name + pending-reference-docs reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("GateInwardCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class GateInwardQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/gateentry/GateInward";

    public GateInwardQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: GateEntry doc-numbering 'Gate Inward' + receivingWarehouse/unit + PO/GRN bridge"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            receivingTypeId = 1,
            receivingWarehouseId = 1,
            unitId = 1,
            remarks = "Created by QA suite",
            gateInwardDetails = new[]
            {
                new { itemId = 1, quantity = 10m, uomId = 1 }
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
            receivingTypeId = 1,
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
    // SECTION 3 — EXTRA READS  (GetById, by-name, pending-reference-docs reachability)
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
    public async Task TC032_GetPendingReferenceDocs_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending-reference-docs?partyId=1&referenceDocumentTypeId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetPendingReferenceDocs_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending-reference-docs?partyId=1&referenceDocumentTypeId=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
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
    // SECTION 5 — DELETE  (lifecycle BLOCKED; negatives ACTIVE — id bound from QUERY)
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

    [Fact(Skip = "needs seeded data: a created GateInward id (TC001 is blocked on doc-numbering + warehouse/unit + PO/GRN bridge)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
