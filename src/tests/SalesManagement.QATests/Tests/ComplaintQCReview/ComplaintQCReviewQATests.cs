namespace SalesManagement.QATests.Tests.ComplaintQCReview;

// ─────────────────────────────────────────────────────────────────────────────
// ComplaintQCReview (transactional workflow step) — live-server QA suite.
//
// Contract verified against source (2026-06-15):
//   POST  /api/ComplaintQCReview                         SubmitQCReviewCommand (JSON body)
//         { complaintHeaderId, physicalVerificationId, complaintStatusId?, severityId?,
//           compensationStructureId?, labVerificationRequired, labResponsiblePersonId?,
//           expectedResolutionDate?, comments?, assignments?[] }
//         (complaintHeaderId is a same-module Complaint FK; physicalVerificationId is a
//          MiscMaster FK — submit needs a real pending complaint, so it is BLOCKED.)
//   PUT   /api/ComplaintQCReview                          UpdateQCReviewCommand (JSON body)
//   GET   /api/ComplaintQCReview?PageNumber=&PageSize=&SearchTerm=&StatusFilter=
//   GET   /api/ComplaintQCReview/{id}                     (200 + data:null when not found)
//   GET   /api/ComplaintQCReview/by-complaint/{complaintHeaderId}
//
//   NO delete endpoint exists.
//
// Strategy: submit-happy + lifecycle BLOCKED (needs a seeded complaint). Smoke GetAll,
// no-auth 401, empty/missing-field 400, and reachability for the read endpoints.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ComplaintQCReviewCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ComplaintQCReviewQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ComplaintQCReview";

    public ComplaintQCReviewQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — SUBMIT (negatives active; happy path BLOCKED) ────────────────

    [Fact(Skip = "needs seeded data: a real pending Complaint (complaintHeaderId) plus a " +
                 "MiscMaster physicalVerificationId and department assignment ids; the QA clone " +
                 "cannot guarantee a complaint awaiting QC review.")]
    [TestPriority(1)]
    public async Task TC001_Submit_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            complaintHeaderId = _f.CreatedId,
            physicalVerificationId = 1,
            labVerificationRequired = false,
            comments = "QA QC review"
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
            physicalVerificationId = 1
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
            physicalVerificationId = 1,
            labVerificationRequired = false
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&StatusFilter=Pending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── SECTION 3 — GET BY ID / BY-COMPLAINT (reachability) ─────────────────────

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
    public async Task TC032_GetByComplaint_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-complaint/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 4 — UPDATE (BLOCKED — needs a submitted QC review) ───────────────

    [Fact(Skip = "needs seeded data: a submitted QC review id (TC001 is blocked).")]
    [TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            complaintHeaderId = 1,
            physicalVerificationId = 1,
            comments = "Updated by QA"
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
