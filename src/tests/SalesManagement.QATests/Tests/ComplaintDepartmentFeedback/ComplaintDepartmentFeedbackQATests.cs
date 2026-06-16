namespace SalesManagement.QATests.Tests.ComplaintDepartmentFeedback;

// ─────────────────────────────────────────────────────────────────────────────
// ComplaintDepartmentFeedback (transactional workflow step) — live-server QA suite.
//
// Contract verified against source (2026-06-15):
//   POST  /api/ComplaintDepartmentFeedback                 SubmitFeedbackCommand (JSON body)
//         { assignmentId, rootCauseText?, rootCauseCategoryId?, correctiveAction?,
//           preventiveAction?, remarks?, attachments?[] }
//         (assignmentId is a same-module ComplaintQCReviewAssignment FK — submit needs a
//          real QC-review assignment, so it is BLOCKED.)
//   PUT   /api/ComplaintDepartmentFeedback                  UpdateFeedbackCommand (JSON body)
//   PUT   /api/ComplaintDepartmentFeedback/request-rework   RequestReworkCommand (JSON body)
//   POST  /api/ComplaintDepartmentFeedback/upload-attachment  [FromForm] multipart
//   DELETE/api/ComplaintDepartmentFeedback/delete-attachment/{id}  (id from ROUTE)
//   GET   /api/ComplaintDepartmentFeedback?PageNumber=&PageSize=&SearchTerm=&StatusFilter=&MyPendingOnly=
//   GET   /api/ComplaintDepartmentFeedback/{id}              (returns ApiResponseDTO — isSuccess/data)
//   GET   /api/ComplaintDepartmentFeedback/by-assignment/{assignmentId}
//   GET   /api/ComplaintDepartmentFeedback/by-complaint/{complaintHeaderId}
//   GET   /api/ComplaintDepartmentFeedback/all-for-reviewer?PageNumber=&PageSize=&SearchTerm=&StatusFilter=
//   GET   /api/ComplaintDepartmentFeedback/by-complaint-full/{complaintHeaderId}
//   GET   /api/ComplaintDepartmentFeedback/my-pending
//
//   NO top-level entity delete endpoint (only delete-attachment).
//
// Strategy: submit-happy + lifecycle BLOCKED (needs a seeded QC-review assignment). Smoke
// GetAll, no-auth 401, empty/missing-field 400, and reachability for the read endpoints.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ComplaintDepartmentFeedbackCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ComplaintDepartmentFeedbackQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ComplaintDepartmentFeedback";

    public ComplaintDepartmentFeedbackQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — SUBMIT (negatives active; happy path BLOCKED) ────────────────

    [Fact(Skip = "needs seeded data: a real ComplaintQCReviewAssignment id (assignmentId) " +
                 "produced by a submitted QC review against a pending complaint; the QA clone " +
                 "cannot guarantee an open department assignment.")]
    [TestPriority(1)]
    public async Task TC001_Submit_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            assignmentId = _f.CreatedId,
            rootCauseText = "QA root cause",
            correctiveAction = "QA corrective",
            preventiveAction = "QA preventive",
            remarks = "QA feedback"
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { assignmentId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Submit_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Submit_MissingAssignmentId_Returns400()
    {
        // assignmentId omitted (0) → required/FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            rootCauseText = "No assignment"
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
    public async Task TC022_GetAll_MyPendingOnly_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&MyPendingOnly=true");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── SECTION 3 — READ ENDPOINT REACHABILITY ──────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetByAssignment_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-assignment/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetByComplaint_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-complaint/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_AllForReviewer_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/all-for-reviewer?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_ByComplaintFull_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-complaint-full/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_MyPending_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/my-pending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 4 — UPDATE / REQUEST-REWORK (BLOCKED happy paths) ────────────────

    [Fact(Skip = "needs seeded data: a submitted feedback id (TC001 is blocked).")]
    [TestPriority(40)]
    public async Task TC040_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            assignmentId = 1,
            rootCauseText = "Updated root cause"
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 1, assignmentId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_RequestRework_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/request-rework", new { id = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(44)]
    public async Task TC044_RequestRework_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/request-rework", new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 5 — ATTACHMENT DELETE (reachability + auth) ──────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_DeleteAttachment_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/delete-attachment/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_DeleteAttachment_NonExistentId_Reachable()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/delete-attachment/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }
}
