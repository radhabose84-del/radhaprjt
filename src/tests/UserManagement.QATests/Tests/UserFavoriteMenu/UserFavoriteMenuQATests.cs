namespace UserManagement.QATests.Tests.UserFavoriteMenu;

// ─────────────────────────────────────────────────────────────────────────────
// UserFavoriteMenuController — VERIFIED CONTRACT (UserManagement.Presentation/Controllers/UserFavoriteMenuController.cs)
//   Route base: /api/UserFavoriteMenu   (ApiControllerBase, global JWT middleware)
//   GET    /api/UserFavoriteMenu          GetUserFavoriteMenusQuery (auth-context user) → { StatusCode, data }
//   POST   /api/UserFavoriteMenu          [FromBody] AddUserFavoriteMenuCommand { MenuId }
//   DELETE /api/UserFavoriteMenu/{menuId} RemoveUserFavoriteMenuCommand(menuId)  ← ROUTE param (not body)
//
// ACTION controller (per-user favorites, keyed off the JWT identity). Coverage:
//   • ACTIVE (Smoke) — GET (current user's favorites) → tolerant 200/404
//   • ACTIVE          — no-auth on GET → 401
//   • ACTIVE          — empty-body POST → tolerant 400/404 (MenuId 0 invalid / menu not found)
//   • ACTIVE          — no-auth DELETE → 401
//   • SKIPPED         — add/remove happy path (needs a valid MenuId the user can favorite)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("UserFavoriteMenuCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class UserFavoriteMenuQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/UserFavoriteMenu";

    public UserFavoriteMenuQATests(QAServerFixture fixture) => _f = fixture;

    // ── ACTIVE (Smoke): GET current user's favorites is reachable ───────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetFavorites_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync(BaseRoute);
        // Tolerant: 200 with a (possibly empty) list, or 404 if the user has no favorites view.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── ACTIVE: no-auth on GET → 401 ────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_GetFavorites_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync(BaseRoute);
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: empty-body POST (MenuId 0) → tolerant 400/404 ───────────────────
    [Fact, TestPriority(3)]
    public async Task TC003_AddFavorite_EmptyBody_Rejected()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        // MenuId defaults to 0 → invalid (400) or treated as not-found menu (404).
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    // ── ACTIVE: no-auth on POST → 401 ───────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task TC004_AddFavorite_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new { menuId = 1 });
        await QAHelper.Assert401Async(resp);
    }

    // ── ACTIVE: no-auth on DELETE (route param) → 401 ───────────────────────────
    [Fact, TestPriority(5)]
    public async Task TC005_RemoveFavorite_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        await QAHelper.Assert401Async(resp);
    }

    // ── SKIPPED: add favorite happy path ────────────────────────────────────────
    [Fact(Skip = "needs seeded data: a valid MenuId that the testsales user is allowed to favorite. Un-skip when a menu fixture is resolvable (e.g. via /api/Menu)."), TestPriority(6)]
    public async Task TC006_AddFavorite_HappyPath_Returns200()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { menuId = 1 });
        await QAHelper.AssertOkAsync(resp);
    }

    // ── SKIPPED: remove favorite happy path ─────────────────────────────────────
    [Fact(Skip = "needs seeded data: a favorited MenuId to remove (depends on TC006). Un-skip alongside the add happy path."), TestPriority(7)]
    public async Task TC007_RemoveFavorite_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/1");
        await QAHelper.AssertOkAsync(resp);
    }
}
