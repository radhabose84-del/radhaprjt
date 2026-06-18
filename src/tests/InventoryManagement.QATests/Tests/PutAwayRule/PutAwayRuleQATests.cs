namespace InventoryManagement.QATests.Tests.PutAwayRule;

// ─────────────────────────────────────────────────────────────────────────────
// PutAwayRule — live-server QA suite (PUT-AWAY config; create + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-17 — PutAwayRulesController.cs):
//   ⚠ Route is "api/[controller]" → /api/PutAwayRule (controller = PutAwayRuleController)
//   GET    /api/PutAwayRule?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/PutAwayRule/{id}
//   GET    /api/PutAwayRule/PutAwayRuleLoad/{ItemIds}/{WarehouseIds}   (csv route params)
//   POST   /api/PutAwayRule   ({ unitId, itemGroupId, itemCategoryId, itemId?, warehouseId,
//                                isActive, strategies:[{storageTypeId,targetId,priorityId,isActive}] })
//   PUT    /api/PutAwayRule
//   DELETE /api/PutAwayRule?id={id}   (id bound from QUERY)
//
// Why create + lifecycle are SKIPPED:
//   A valid put-away rule requires seeded unit / itemGroup / itemCategory / warehouse /
//   storageType / target ids — none guaranteed on the QA clone. Attribute-level [Fact(Skip=...)].
//   GetAll (smoke), PutAwayRuleLoad reachability, and negatives remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PutAwayRuleCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PutAwayRuleQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PutAwayRule";

    public PutAwayRuleQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path + lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: unit/itemGroup/itemCategory/warehouse/storageType/target"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            unitId = 1,
            itemGroupId = 1,
            itemCategoryId = 1,
            warehouseId = 1,
            isActive = 1,
            strategies = new[]
            {
                new { storageTypeId = 1, targetId = 1, priorityId = 1, isActive = 1 }
            }
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
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            unitId = 1,
            itemGroupId = 1,
            warehouseId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; tolerant 200/404)
    // ─────────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — EXTRA READS  (GetById + PutAwayRuleLoad reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_PutAwayRuleLoad_Reachable_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/PutAwayRuleLoad/1/1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_PutAwayRuleLoad_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/PutAwayRuleLoad/1/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE / DELETE  (lifecycle BLOCKED; negatives ACTIVE — id from QUERY)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: unit/itemGroup/itemCategory/warehouse/storageType/target"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            unitId = 1,
            itemGroupId = 1,
            itemCategoryId = 1,
            warehouseId = 1,
            isActive = 1,
            strategies = new[]
            {
                new { storageTypeId = 1, targetId = 1, priorityId = 1, isActive = 1 }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 999999, unitId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: unit/itemGroup/itemCategory/warehouse/storageType/target"), TestPriority(91)]
    public async Task TC091_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
