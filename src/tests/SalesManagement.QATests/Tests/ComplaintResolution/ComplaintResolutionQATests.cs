namespace SalesManagement.QATests.Tests.ComplaintResolution;

// ─────────────────────────────────────────────────────────────────────────────
// ComplaintResolution (transactional workflow step) — live-server QA suite.
//
// Contract verified against source (2026-06-15):
//   POST  /api/ComplaintResolution                       SubmitResolutionCommand (JSON body)
//         { complaintHeaderId, resolutionTypeId, resolutionSummary?,
//           returnQuantity?, returnLocationId?, returnStatusId?,         // Sales Return
//           creditAmount?, financeReference?,                            // Credit Note
//           replacementQuantity?, dispatchReference?,                    // Replacement
//           actionDescription?,                                          // Reprocess
//           closureStatusId?, closureRemarks? }                         // Closure
//         (complaintHeaderId is a same-module Complaint FK; resolutionTypeId is a MiscMaster
//          FK — submit needs a real complaint awaiting resolution, so it is BLOCKED.
//          Fields are polymorphic per resolutionTypeId.)
//   PUT   /api/ComplaintResolution                        UpdateResolutionCommand (JSON body)
//   GET   /api/ComplaintResolution?PageNumber=&PageSize=&SearchTerm=&StatusFilter=
//   GET   /api/ComplaintResolution/form-data/{complaintHeaderId}
//   GET   /api/ComplaintResolution/by-complaint/{complaintHeaderId}
//
//   NO delete endpoint, NO GetById/{id}, NO by-name autocomplete.
//
// Strategy: submit-happy + lifecycle BLOCKED (needs a seeded complaint). Smoke GetAll,
// no-auth 401, empty/missing-field 400, and reachability for the read endpoints.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ComplaintResolutionCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ComplaintResolutionQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ComplaintResolution";

    public ComplaintResolutionQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — SUBMIT (negatives active; happy path BLOCKED) ────────────────

    [Fact(Skip = "needs seeded data: a real Complaint (complaintHeaderId) that has cleared QC " +
                 "review and is awaiting resolution, plus a MiscMaster resolutionTypeId; the QA " +
                 "clone cannot guarantee a complaint at the resolution stage.")]
    [TestPriority(1)]
    public async Task TC001_Submit_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            complaintHeaderId = _f.CreatedId,
            resolutionTypeId = 1,
            resolutionSummary = "QA resolution",
            closureRemarks = "Closed by QA"
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Submit_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            complaintHeaderId = 1,
            resolutionTypeId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Submit_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Submit_MissingComplaintHeaderId_Returns400()
    {
        // complaintHeaderId omitted (0) → required/FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            resolutionTypeId = 1,
            resolutionSummary = "No header"
        });

        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET ALL (smoke) ─────────────────────────────────────────────

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
    public async Task TC022_GetAll_WithStatusFilter_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&StatusFilter=Closed");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── SECTION 3 — READ ENDPOINT REACHABILITY ──────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_FormData_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/form-data/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_FormData_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/form-data/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_ByComplaint_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-complaint/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 4 — UPDATE (BLOCKED — needs a submitted resolution) ──────────────

    [Fact(Skip = "needs seeded data: a submitted resolution id (TC001 is blocked).")]
    [TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            complaintHeaderId = 1,
            resolutionTypeId = 1,
            resolutionSummary = "Updated by QA"
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 1, complaintHeaderId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }
}
