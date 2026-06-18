namespace PurchaseManagement.QATests.Tests.PurchaseOrderPrint;

// ─────────────────────────────────────────────────────────────────────────────
// PurchaseOrderPrint — live-server QA suite (READ-ONLY print details; reachability).
//
// Contract verified against source (2026-06-17 — PurchaseOrderPrintController.cs):
//   Route prefix: [Route("api/[controller]")] → /api/PurchaseOrderPrint
//   GET    /api/PurchaseOrderPrint/{id}    (always Ok(); data null when PO not found)
//
// Why reachability only:
//   Read-only print projection of an existing PO. The clone has no guaranteed PO id, so we hit
//   id=1 and assert reachability tolerantly (200/400/404).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PurchaseOrderPrintCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PurchaseOrderPrintQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PurchaseOrderPrint";

    public PurchaseOrderPrintQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetById_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 400);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
