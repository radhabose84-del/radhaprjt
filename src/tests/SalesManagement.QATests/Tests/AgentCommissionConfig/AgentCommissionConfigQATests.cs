namespace SalesManagement.QATests.Tests.AgentCommissionConfig;

// ─────────────────────────────────────────────────────────────────────────────
// AgentCommissionConfig — live-server QA suite (master + nested slabs CRUD + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/AgentCommissionConfig
//          {
//            agentId, commissionTypeId, commissionBasisId, applicableLevelId,
//            commissionPercentage(>0), validityFrom(DateTimeOffset), validityTo?,
//            triggerEventId, slabTypeId?, commissionSplitId,
//            salesGroupIds?:[int], paymentTermIds?:[int],
//            slabs:[{ slabOrder, fromDelay, toDelay?, commissionTypeId,
//                     commissionBasisId, commissionValue }]
//          }
//   PUT    /api/AgentCommissionConfig   (same shape + id, isActive)
//   DELETE /api/AgentCommissionConfig?id={id}   (id bound from QUERY — action param `int id`)
//   GET    /api/AgentCommissionConfig?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/AgentCommissionConfig/{id}      (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/AgentCommissionConfig/by-name?term=
//
// Nested-detail / FK rules (from CreateAgentCommissionConfigCommandValidator):
//   • agentId / commissionTypeId / commissionBasisId / applicableLevelId / triggerEventId /
//     commissionSplitId are all NotEmpty (>0). commissionPercentage > 0. validityFrom != default.
//   • At least one slab required ("At least one slab is required."); each slab CommissionTypeId,
//     CommissionBasisId, CommissionValue > 0 and FromDelay >= 0.
//   • commissionTypeId/basis/applicableLevel/triggerEvent/slabType + slab type/basis are same-module
//     MiscMaster FKs (MiscMasterExistsAsync). commissionSplitId → CommissionSplitExistsAsync.
//     agentId is a CROSS-MODULE Party FK (AgentExistsAsync). validityTo >= validityFrom.
//   • OverlapExistsAsync: an active rule for the same Agent+CommissionSplit+validity window is rejected.
//
// FK / seeding note:
//   A valid create needs a real Party agentId AND a real CommissionSplit id — neither is guaranteed
//   in the QA clone (CommissionSplit creation itself is skipped for lack of two distinct MiscMaster
//   role ids). So create-happy and every create-dependent test are wrapped in
//   [Fact(Skip="needs seeded data: …")]. Reachability/negative/GetAll(Smoke) tests stay ACTIVE.
//
// Delete action binds `int id` from query (?id=). GetById has no null guard → 200+data:null.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AgentCommissionConfigCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AgentCommissionConfigQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AgentCommissionConfig";
    private const string MiscMasterRoute = "/api/sales/MiscMaster";
    private const string CommissionSplitRoute = "/api/CommissionSplit";

    private static int _miscId;

    public AgentCommissionConfigQATests(QAServerFixture fixture) => _f = fixture;

    private async Task<int> MiscIdAsync()
    {
        if (_miscId == 0)
            _miscId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        return _miscId > 0 ? _miscId : 1; // fallback 1 per spec
    }

    private static string IsoNow() => DateTimeOffset.UtcNow.ToString("o");
    private static string IsoNowPlusYear() => DateTimeOffset.UtcNow.AddYears(1).ToString("o");

    // Builds a full, valid create payload — used by the (skipped) happy paths.
    private async Task<object> BuildValidCreateBody(int agentId, int commissionSplitId)
    {
        var misc = await MiscIdAsync();
        return new
        {
            agentId,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            validityTo = IsoNowPlusYear(),
            triggerEventId = misc,
            slabTypeId = (int?)misc,
            commissionSplitId,
            slabs = new[]
            {
                new
                {
                    slabOrder = 1,
                    fromDelay = 0,
                    toDelay = (int?)30,
                    commissionTypeId = misc,
                    commissionBasisId = misc,
                    commissionValue = 2m
                }
            }
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a real Party agentId + a real CommissionSplit id — not guaranteed in the QA clone"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        // If ever un-skipped: resolve a live agent (cross-module Party) and CommissionSplit first.
        var agentId = await QAHelper.FirstIdAsync(_f.Client, "/api/Agent");
        var splitId = await QAHelper.FirstIdAsync(_f.Client, CommissionSplitRoute);
        if (agentId == 0) agentId = 1;
        if (splitId == 0) splitId = 1;

        var body = await BuildValidCreateBody(agentId, splitId);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, body);

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var body = await BuildValidCreateBody(1, 1);
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, body);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MissingRequiredFks_Returns400()
    {
        // All *Id default 0 → GreaterThan(0) NotEmpty rules fail.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = 1, commissionBasisId = 1, commissionValue = 2m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CommissionPercentageZero_Returns400()
    {
        var misc = await MiscIdAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 0m,
            validityFrom = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 2m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_MissingSlabs_Returns400()
    {
        // "At least one slab is required."
        var misc = await MiscIdAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            slabs = Array.Empty<object>()
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "slab");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_SlabCommissionValueZero_Returns400()
    {
        var misc = await MiscIdAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 0m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_InvalidAgentFk_Returns400()
    {
        // AgentExistsAsync(999999) → false (cross-module Party FK).
        var misc = await MiscIdAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            agentId = 999999,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 2m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_InvalidMiscMasterFk_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            agentId = 1,
            commissionTypeId = 999999,
            commissionBasisId = 999999,
            applicableLevelId = 999999,
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            triggerEventId = 999999,
            commissionSplitId = 999999,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = 999999, commissionBasisId = 999999, commissionValue = 2m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_ValidityToBeforeFrom_Returns400()
    {
        // DateCompare: validityTo must be >= validityFrom.
        var misc = await MiscIdAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 5m,
            validityFrom = IsoNowPlusYear(),
            validityTo = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 2m }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (no null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created AgentCommissionConfig (TC001 is skipped)"), TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created AgentCommissionConfig (TC001 is skipped)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var misc = await MiscIdAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 7m,
            validityFrom = IsoNow(),
            validityTo = IsoNowPlusYear(),
            triggerEventId = misc,
            slabTypeId = (int?)misc,
            commissionSplitId = 1,
            isActive = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, toDelay = (int?)30, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 3m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            agentId = 1,
            commissionTypeId = 1,
            commissionBasisId = 1,
            applicableLevelId = 1,
            commissionPercentage = 5m,
            validityFrom = IsoNow(),
            triggerEventId = 1,
            commissionSplitId = 1,
            isActive = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = 1, commissionBasisId = 1, commissionValue = 2m }
            }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact(Skip = "needs seeded data: a successfully created AgentCommissionConfig (TC001 is skipped)"), TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var misc = await MiscIdAsync();

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 7m,
            validityFrom = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            isActive = 0,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 3m }
            }
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            agentId = 1,
            commissionTypeId = misc,
            commissionBasisId = misc,
            applicableLevelId = misc,
            commissionPercentage = 7m,
            validityFrom = IsoNow(),
            triggerEventId = misc,
            commissionSplitId = 1,
            isActive = 1,
            slabs = new[]
            {
                new { slabOrder = 1, fromDelay = 0, commissionTypeId = misc, commissionBasisId = misc, commissionValue = 3m }
            }
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a successfully created AgentCommissionConfig (TC001 is skipped)"), TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact(Skip = "needs seeded data: a successfully created+deleted AgentCommissionConfig (TC001/TC093 are skipped)"), TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }
}
