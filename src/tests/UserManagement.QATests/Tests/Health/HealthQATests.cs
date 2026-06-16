// ─────────────────────────────────────────────────────────────────────────────
// Health — live-server QA tests (READ-ONLY: single GET endpoint)
//
// Controller: UserManagement.Presentation.Controllers.HealthController
// Route attr: [Route("api/[controller]")]  →  base route /api/Health
//
// Verified endpoint (from controller source):
//   GET  /api/Health   Get()  →  Ok("Healthy")   (plain string body)
//
// AUTH NOTE — VERIFIED: HealthController is NOT decorated with [AllowAnonymous],
// and it has no [Authorize] either (auth is enforced globally by
// TokenValidationMiddleware, which only bypasses /notificationHub and endpoints
// carrying IAllowAnonymous metadata). Therefore /api/Health DOES require a bearer
// token on the live server: the smoke happy-path uses the authenticated client,
// and a no-auth 401 test IS included (it is not anonymous).
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.Health;

[Collection("HealthCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class HealthQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Health";

    public HealthQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_Health_HappyPath_Returns200_Healthy()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);

        await QAHelper.AssertOkAsync(resp);
        var body = await resp.Content.ReadAsStringAsync();
        // Body is the plain string "Healthy" (possibly JSON-quoted) — match tolerantly.
        body.Should().Contain("Healthy");
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Health_NoAuthToken_Returns401()
    {
        // Health is not [AllowAnonymous] → global token middleware rejects anonymous calls.
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        await QAHelper.Assert401Async(resp);
    }
}
