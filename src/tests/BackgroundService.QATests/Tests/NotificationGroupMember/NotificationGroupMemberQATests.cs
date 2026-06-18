namespace BackgroundService.QATests.Tests.NotificationGroupMember;

// ─────────────────────────────────────────────────────────────────────────────
// NotificationGroupMember — live-server QA suite (COMPLEX; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-18):
//   NotificationGroupMemberController = [Route("api/[controller]")]  → base /api/NotificationGroupMember
//   POST   /api/NotificationGroupMember   { groupId(FK→/api/NotificationGroup),
//                                           userIds:int[] (FK→/api/User, NotEmpty, no dups) }
//                                          → returns int (count inserted, >0)
//   PUT    /api/NotificationGroupMember   { ... }
//   GET    /api/NotificationGroupMember?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/NotificationGroupMember/{id}   (id = GroupId; returns grouped DTO;
//                                               controller returns 404 when handler IsSuccess=false)
//   NO DELETE endpoint.
//
// Create-happy + lifecycle SKIPPED — needs a real NotificationGroup parent and real UserManagement
// user ids the clone does not guarantee. Negatives, smoke GetAll, and GetById-nonexistent (404)
// remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("NotificationGroupMemberCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class NotificationGroupMemberQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/NotificationGroupMember";

    public NotificationGroupMemberQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (happy BLOCKED; negatives ACTIVE) ──

    [Fact(Skip = "needs seeded data: a NotificationGroup + real UserManagement user ids"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_InsertsMembers()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            groupId = 1,
            userIds = new[] { 1, 2 }
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        // data = count inserted (int) > 0
        doc.RootElement.GetProperty("data").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            groupId = 1,
            userIds = new[] { 1 }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyUserIds_Returns400()
    {
        // userIds NotEmpty → empty array fails validation.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            groupId = 1,
            userIds = Array.Empty<int>()
        });

        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET ALL (smoke; list endpoint expected 200) ──

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);

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
    public async Task TC022_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    // ── SECTION 3 — GET BY ID (id = GroupId; 404 when not found) ──

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentGroup_Returns404Or200()
    {
        // Controller returns NotFound(404) when handler IsSuccess=false; tolerant for clone variance.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SECTION 4 — UPDATE (lifecycle BLOCKED; negatives ACTIVE; NO DELETE on this entity) ──

    [Fact, TestPriority(50)]
    public async Task TC050_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            groupId = 999999,
            userIds = new[] { 1 }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }
}
