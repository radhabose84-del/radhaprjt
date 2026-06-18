namespace PurchaseManagement.QATests.Tests.GateEntry;

// ─────────────────────────────────────────────────────────────────────────────
// GateEntry — live-server QA suite (TRANSACTIONAL; create-happy SKIPPED).
//
// Contract verified against source (2026-06-17 — GateEntryController.cs,
// [Route("api/[controller]")] => /api/GateEntry):
//   POST   /api/GateEntry                  CreateGateEntryCommand
//   GET    /api/GateEntry/{partyId}        (approved-PO list for a party)
//   POST   /api/GateEntry/upload-document  (multipart staging — out of scope)
//   DELETE /api/GateEntry/delete-document  (out of scope)
//
// Why create-happy is SKIPPED:
//   A valid GateEntry needs a seeded unit/party plus an APPROVED PO — none guaranteed on
//   the QA clone. The primary read is GET /{partyId}; negatives remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurGateEntryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class GateEntryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/GateEntry";

    public GateEntryQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ────────────────────

    [Fact(Skip = "needs seeded data: unit/party + approved PO"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            partyId = 1,
            poId = 1,
            gateEntryDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            gateEntryDetails = new[] { new { itemId = 1, quantity = 10m } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { partyId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET BY PARTY ID (smoke; tolerant 200/404) ──────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetByPartyId_HappyPath_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetByPartyId_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetByPartyId_NonExistent_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }
}
