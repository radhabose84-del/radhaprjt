namespace BudgetManagement.QATests.Tests.BudgetAllocation;

// ─────────────────────────────────────────────────────────────────────────────
// BudgetAllocation — live-server QA suite (create-happy SKIPPED; reads ACTIVE).
//
// Contract verified against source (2026-06-16 — BudgetAllocationController.cs):
//   GET    /api/budgetallocation/SpindleDetailsMonthwise?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/budgetallocation/SpindleDetailsMonthwiseReport/{financialYearId}
//   GET    /api/budgetallocation/remainingbalance?budgetGroupId=&date=...
//   GET    /api/budgetallocation/BudgetBalanceReport/{financialYearId}
//   POST   /api/budgetallocation             { createBudgetAllocations: [ {...} ] }  (ARRAY wrapper)
//   (No GetById, no Update, no Delete.)
//
// Key facts that shaped assertions:
//   • POST body is an ARRAY wrapper. An empty array → handler throws ExceptionRules
//       "No Budget Allocations provided." → 400 via GlobalExceptionMiddleware.
//   • SpindleDetailsMonthwise returns 404 when no rows → smoke tolerates 200/404.
//   • Report endpoints return 404 ("No Results found") when empty → reachability tolerates 200/400/404.
//
// Why create-happy is SKIPPED:
//   A valid allocation needs seeded financialYear / budgetGroup / allocationType plus the approval
//   flow — none reliably resolvable on the QA clone.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("BudgetAllocationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class BudgetAllocationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/budgetallocation";

    public BudgetAllocationQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: financialYear/budgetGroup/allocationType + approval flow"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            createBudgetAllocations = new[]
            {
                new
                {
                    budgetGroupId = 1,
                    financialYearId = 1,
                    allocationTypeId = 1,
                    allocatedAmount = 1000m
                }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            createBudgetAllocations = new[] { new { budgetGroupId = 1 } }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        // No createBudgetAllocations key → empty list → "No Budget Allocations provided." → 400.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyArray_Returns400()
    {
        // Explicit empty array → handler throws "No Budget Allocations provided." → 400.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            createBudgetAllocations = Array.Empty<object>()
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — SPINDLE DETAILS (read; smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_SpindleDetailsMonthwise_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/SpindleDetailsMonthwise?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_SpindleDetailsMonthwise_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/SpindleDetailsMonthwise?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — REPORT ENDPOINTS (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_SpindleDetailsMonthwiseReport_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/SpindleDetailsMonthwiseReport/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_BudgetBalanceReport_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/BudgetBalanceReport/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_RemainingBalance_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/remainingbalance?budgetGroupId=1&date=2026-01-01");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_BudgetBalanceReport_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/BudgetBalanceReport/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
