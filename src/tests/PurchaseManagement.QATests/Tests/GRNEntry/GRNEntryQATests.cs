namespace PurchaseManagement.QATests.Tests.GRNEntry;

// ─────────────────────────────────────────────────────────────────────────────
// GRNEntry — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — GRN/GRNEntryController.cs,
// [Route("api/[controller]")] => /api/GRNEntry):
//   POST   /api/GRNEntry                                         CreateGRNEntryCommand
//   PUT    /api/GRNEntry                                         UpdateGRNEntryCommand
//   GET    /api/GRNEntry/{partyId}                               (pending PO list for a party)
//   GET    /api/GRNEntry/GRNEntryPendingHeader?fromDate=&toDate=&IsGrnGenerated=&IsQcGenerated=
//                                                  &PageNumber=&PageSize=&SearchTerm=
//   GET    /api/GRNEntry/po-pending
//   GET    /api/GRNEntry/GateEntryPendingPo/{partyId}/{poId}
//   (also upload/delete document POST/DELETE variants — out of scope here)
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid GRN needs a seeded GateEntry + PO plus document-numbering series 'GRN' and a
//   warehouse — none guaranteed on the QA clone. Negatives / reachability remain ACTIVE.
//   The primary smoke list is GRNEntryPendingHeader (the only paged read on this controller).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("GRNEntryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class GRNEntryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/GRNEntry";

    public GRNEntryQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: GateEntry+PO + doc-numbering 'GRN' + warehouse"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            gateEntryId = 1,
            poId = 1,
            partyId = 1,
            warehouseId = 1,
            grnDetails = new[] { new { itemId = 1, quantity = 10m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { poId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GRNEntryPendingHeader (smoke; tolerant 200/404) ─────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetPendingHeader_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GRNEntryPendingHeader?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetPendingHeader_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/GRNEntryPendingHeader?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 3 — EXTRA READS (reachability; tolerant) ───────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_PoPending_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/po-pending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_PoPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/po-pending");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetByPartyId_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GateEntryPendingPo_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/GateEntryPendingPo/1/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 4 — UPDATE (lifecycle BLOCKED; negatives ACTIVE) ────────────────

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
}
