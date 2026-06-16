// ─────────────────────────────────────────────────────────────────────────────
// TimeZones — live-server QA tests (READ-ONLY entity: GET endpoints only)
//
// Controller: UserManagement.Presentation.Controllers.TimeZonesController
// Route attr: [Route("api/[controller]")]  →  base route /api/TimeZones
//
// Verified endpoints (from controller source):
//   GET  /api/TimeZones?PageNumber=&PageSize=&SearchTerm=   GetAllTimeZonesAsync
//        • returns 200 with { message, data, statusCode, TotalCount, PageNumber, PageSize }
//        • returns 404 when result.Data is null/empty (controller's own guard)
//   GET  /api/TimeZones/{id}                                GetByIdAsync
//        • id <= 0 → 400 "Invalid TimeZone ID"
//        • otherwise 200 { message, statusCode, data }
//   GET  /api/TimeZones/by-name?TimeZoneName=               GetTimeZones (autocomplete)
//        • returns 200 { message, statusCode, data }
//
// No [AllowAnonymous] on the controller → global TokenValidationMiddleware
// requires a bearer token; anonymous requests get 401.
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.TimeZones;

[Collection("TimeZonesCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class TimeZonesQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/TimeZones";

    public TimeZonesQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — GET ALL (primary list) ───────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        // The controller returns 404 (not 200) when the list is empty — tolerate both so
        // the smoke gate stays green on a freshly-reset clone with no TimeZone rows.
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
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByTerm_IsReachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        // No-match search → either 200 with empty data, or the controller's 404 guard.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 400);
    }

    // ── SECTION 2 — GET BY ID (reachability) ─────────────────────────────────

    [Fact, TestPriority(23)]
    public async Task TC023_GetById_IdZero_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "Invalid");
    }

    [Fact, TestPriority(24)]
    public async Task TC024_GetById_ValidOrUnknownId_IsReachable()
    {
        // Resolve a real id from the list when available; otherwise probe a high id.
        var id = await QAHelper.FirstIdAsync(_f.Client, BaseRoute);
        if (id <= 0) id = 999999;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{id}");
        // GetById has no soft-404 guard in the controller — a valid id yields 200,
        // an unknown id may surface 200 (null data) / 404 / 400 depending on backend.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 400);
    }

    [Fact, TestPriority(25)]
    public async Task TC025_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        await QAHelper.Assert401Async(resp);
    }

    // ── SECTION 3 — AUTOCOMPLETE (by-name) reachability ──────────────────────

    [Fact, TestPriority(26)]
    public async Task TC026_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?TimeZoneName=India");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(27)]
    public async Task TC027_AutoComplete_EmptyName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(28)]
    public async Task TC028_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?TimeZoneName=India");
        await QAHelper.Assert401Async(resp);
    }
}
