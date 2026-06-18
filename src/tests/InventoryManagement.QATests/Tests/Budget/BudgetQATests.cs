namespace InventoryManagement.QATests.Tests.Budget;

// ─────────────────────────────────────────────────────────────────────────────
// Budget (Inventory) — live-server QA suite (create + update SKIPPED).
//
// Contract verified against source (2026-06-17 — BudgetController.cs):
//   ⚠ Route is "api/[controller]" → /api/Budget
//   GET    /api/Budget/{id}
//   POST   /api/Budget/create        ([FromBody] CreateBudgetCommand — budgetGroupId + nested budgetDetails)
//   PUT    /api/Budget/update        ([FromBody] UpdateBudgetCommand — requires budgetId > 0)
//   GET    /api/Budget/getall?fiscalYear=
//   GET    /api/Budget/logs?budgetId=&budgetDetailId=
//
// Why create + update are SKIPPED:
//   A valid budget requires an external budgetGroupId plus monthly nested budgetDetails — neither
//   guaranteed on the QA clone. Attribute-level [Fact(Skip=...)].
//   getall (smoke), GetById non-existent, logs reachability, and negatives remain ACTIVE.
//   Note: POST /create returns BadRequest("Invalid budget data.") for a null command body → 400.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("InvBudgetCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class BudgetQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Budget";

    public BudgetQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path + update BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: budgetGroupId + monthly budgetDetails"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            budgetGroupId = 1,
            fiscalYear = 2026,
            budgetDetails = new[]
            {
                new { month = 1, amount = 1000m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.GetProperty("budgetId").GetInt32();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{BaseRoute}/create", new
        {
            budgetGroupId = 1,
            fiscalYear = 2026
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/create", new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/getall?fiscalYear=2026");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/getall?fiscalYear=2026");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (GetById + logs reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_Logs_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/logs");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Logs_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/logs");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: budgetGroupId + monthly budgetDetails"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update", new
        {
            budgetId = _f.CreatedId,
            fiscalYear = 2026,
            budgetDetails = new[]
            {
                new { month = 1, amount = 2000m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/update", new { budgetId = 999999, fiscalYear = 2026 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
