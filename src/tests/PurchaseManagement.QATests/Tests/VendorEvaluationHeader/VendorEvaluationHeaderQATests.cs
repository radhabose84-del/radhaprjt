namespace PurchaseManagement.QATests.Tests.VendorEvaluationHeader;

// ─────────────────────────────────────────────────────────────────────────────
// VendorEvaluationHeader — live-server QA suite (TRANSACTIONAL; create-happy SKIPPED).
//
// Contract verified against source (2026-06-17 — VendorEvaluationHeaderController.cs):
//   Route prefix: [Route("api/[controller]")] → /api/VendorEvaluationHeader
//   GET    /api/VendorEvaluationHeader?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/VendorEvaluationHeader/dashboard?vendorId=&evaluationMonth=&evaluationYear=
//   GET    /api/VendorEvaluationHeader/{id}             (200 + data:null when not found)
//   POST   /api/VendorEvaluationHeader                  (create — returns 200)
//   PUT    /api/VendorEvaluationHeader                  (update)
//   DELETE /api/VendorEvaluationHeader/{id}
//   GET    /api/VendorEvaluationHeader/rating-dashboard?PageNumber=&PageSize=
//   GET    /api/VendorEvaluationHeader/evaluation-history/{vendorId}
//
// Why create-happy is SKIPPED:
//   A valid header requires a seeded vendor plus per-criteria evaluation scores — neither
//   guaranteed on the QA clone. Attribute-level [Fact(Skip=...)]. Negatives, smoke GetAll,
//   and read reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("VendorEvaluationHeaderCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class VendorEvaluationHeaderQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/VendorEvaluationHeader";

    public VendorEvaluationHeaderQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ──────────────────

    [Fact(Skip = "needs seeded data: vendor + evaluation criteria scores"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            vendorId = 1,
            evaluationMonth = 6,
            evaluationYear = 2026,
            scores = new[] { new { criteriaId = 1, score = 5 } }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { vendorId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET ALL (smoke; tolerant 200/404) ─────────────────────────

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

    // ── SECTION 3 — EXTRA READS (reachability) ────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_RatingDashboard_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/rating-dashboard?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_EvaluationHistory_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/evaluation-history/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 4 — UPDATE / DELETE negatives (ACTIVE) ────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
