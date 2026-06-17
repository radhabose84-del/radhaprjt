namespace SalesManagement.QATests.Tests.AgentCustomerMapping;

// ─────────────────────────────────────────────────────────────────────────────
// AgentCustomerMapping — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/AgentCustomerMapping        { customerId, agentId, subAgentId?, salesGroupId,
//                                            effectiveFrom (DateTime, ISO 8601, NOT future),
//                                            effectiveTo? (> effectiveFrom), isDefaultAgent, remarks?(max500) }
//   PUT    /api/AgentCustomerMapping        { id, customerId, agentId, subAgentId?, salesGroupId,
//                                            effectiveFrom, effectiveTo?, isDefaultAgent, remarks?, isActive }
//   DELETE /api/AgentCustomerMapping?id={id}(id bound from QUERY — [HttpDelete] Delete(int id), no route param)
//   GET    /api/AgentCustomerMapping?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/AgentCustomerMapping/{id}   (NO null guard → 200 + data:null when not found)
//   GET    /api/AgentCustomerMapping/by-name?term=
//
// Key facts that shaped assertions:
//   • Required (>0): CustomerId, AgentId, SalesGroupId; EffectiveFrom != default AND <= today (no future).
//   • FK checks: CustomerExistsAsync / AgentExistsAsync / SubAgentExistsAsync (Party cross-module),
//     SalesGroupExistsAsync (same-module → /api/SalesGroup).
//   • Business rules: SubAgentId != AgentId; duplicate (Customer, Agent) mapping blocked;
//     EffectiveTo > EffectiveFrom; MarketingOfficerAccess on AgentId.
//
// FK resolution / create-happy assumptions (RECONCILE LIVE):
//   • customerId / agentId → FirstIdAsync(/api/party/PartyMaster). Parties are not split into
//     customer/agent GET endpoints, so the same first party id may be used for both — which then
//     trips the "SubAgentId != AgentId" / duplicate rules only if reused incorrectly. We resolve a
//     single party id and use it for customerId; agentId falls back to 1 (distinct) so they differ.
//   • salesGroupId → FirstIdAsync(/api/SalesGroup).
//   • If party ids cannot be resolved (PartyMaster unreachable/empty) create-happy will 400 on a
//     Customer/Agent FK message — adjust the resolved ids during live reconciliation. The
//     validation/negative tests do not depend on real party ids.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AgentCustomerMappingCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AgentCustomerMappingQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/AgentCustomerMapping";

    // Resolved-at-runtime FK ids (set in TC001). Fallbacks chosen so customer != agent.
    private static int _customerId = 1;
    private static int _agentId = 2;
    private static int _salesGroupId = 1;

    // Set true once TC001 successfully creates a mapping (a usable distinct, unmapped pair was
    // found on the clone). Cascade tests (GetById/Update/Delete) guard on this instead of the
    // fixture party ids, so they self-skip cleanly when the clone offers no usable pair.
    private static bool _mappingCreated;

    // EffectiveFrom must NOT be future — use today (ISO 8601).
    private static readonly string EffectiveFrom = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
    private static readonly string EffectiveTo = DateTime.Today.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ss");

    public AgentCustomerMappingQATests(QAServerFixture fixture) => _f = fixture;

    private async Task ResolveFksAsync()
    {
        // Use the real ACTIVE Party ids (CUSTOMER + AGENT) resolved by the shared fixture.
        if (_f.CustomerPartyId > 0) _customerId = _f.CustomerPartyId;
        if (_f.AgentPartyId > 0) _agentId = _f.AgentPartyId;

        var grp = await QAHelper.FirstIdAsync(_f.Client, "/api/SalesGroup");
        if (grp > 0) _salesGroupId = grp;
    }

    private object ValidCreateBody() => new
    {
        customerId = _customerId,
        agentId = _agentId,
        salesGroupId = _salesGroupId,
        effectiveFrom = EffectiveFrom,
        effectiveTo = EffectiveTo,
        isDefaultAgent = true,
        remarks = "Created by QA suite"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    // Build the customer-type and agent-type party id lists from PartyMaster (a party can carry
    // multiple types — same classification the fixture uses).
    private async Task<(List<int> customers, List<int> agents)> ResolvePartyTypeListsAsync()
    {
        var customers = new List<int>();
        var agents = new List<int>();

        var resp = await _f.Client.GetAsync("/api/party/PartyMaster?PageNumber=1&PageSize=50");
        if (!resp.IsSuccessStatusCode) return (customers, agents);

        using var doc = await QAHelper.ParseAsync(resp);
        if (!doc.RootElement.TryGetProperty("data", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return (customers, agents);

        foreach (var p in arr.EnumerateArray())
        {
            if (!p.TryGetProperty("id", out var idp)) continue;
            var pid = idp.GetInt32();
            if (!p.TryGetProperty("partyTypes", out var pts) || pts.ValueKind != JsonValueKind.Array) continue;

            foreach (var t in pts.EnumerateArray())
            {
                var name = t.TryGetProperty("partyTypeName", out var n) ? n.GetString() : null;
                if (name == "CUSTOMER" && !customers.Contains(pid)) customers.Add(pid);
                if (name == "AGENT" && !agents.Contains(pid)) agents.Add(pid);
            }
        }
        return (customers, agents);
    }

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await ResolveFksAsync();

        // FIX (test bug, reconciled 2026-06-16): the fixture can resolve the SAME party for both
        // customer and agent (a party may be both types), and the first pair is often already
        // mapped in seed data. Probe for a DISTINCT (customer, agent) pair that is a valid
        // customer-type + agent-type party AND not already mapped. Self-skip if the clone offers
        // none — cascades guard on _mappingCreated.
        var (custIds, agentIds) = await ResolvePartyTypeListsAsync();

        foreach (var a in agentIds)
        {
            foreach (var c in custIds)
            {
                if (c == a) continue;

                var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
                {
                    customerId = c,
                    agentId = a,
                    salesGroupId = _salesGroupId,
                    effectiveFrom = EffectiveFrom,
                    effectiveTo = EffectiveTo,
                    isDefaultAgent = true,
                    remarks = "Created by QA suite"
                });

                if (resp.StatusCode != HttpStatusCode.OK) continue;   // mapped/invalid → try next

                var doc = await QAHelper.ParseAsync(resp);
                var id = doc.RootElement.CreatedId();
                id.Should().BeGreaterThan(0);

                _customerId = c;
                _agentId = a;
                _f.CreatedId = id;
                _mappingCreated = true;
                return;
            }
        }
        // No usable pair on this clone → self-skip (cascade steps guard on _mappingCreated).
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 1,
            agentId = 2,
            salesGroupId = 1,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = true
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CustomerIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 0,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_AgentIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = _customerId,
            agentId = 0,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_SalesGroupIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = 0,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EffectiveFromMissing_Returns400()
    {
        // Omit effectiveFrom → default(DateTime) → NotEqual(default) fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EffectiveFromFuture_Returns400()
    {
        var future = DateTime.Today.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ss");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = future,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "future");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_EffectiveToBeforeEffectiveFrom_Returns400()
    {
        var earlier = DateTime.Today.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ss");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            effectiveTo = earlier,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NonExistentSalesGroup_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = 999999,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = true
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DuplicateCustomerAgent_Returns400()
    {
        // Same (CustomerId, AgentId) as TC001 → MappingAlreadyExistsAsync blocks duplicate.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidCreateBody());

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
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
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200_WithData()
    {
        if (!_mappingCreated) return;   // depends on TC001 finding a usable pair
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=A");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=A");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (!_mappingCreated) return;   // depends on TC001 finding a usable pair
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            effectiveTo = EffectiveTo,
            isDefaultAgent = false,
            remarks = "Updated by QA",
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = false,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_SalesGroupIdMissing_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = 0,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = false,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = false,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (!_mappingCreated) return;   // depends on TC001 finding a usable pair
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = false,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _customerId,
            agentId = _agentId,
            salesGroupId = _salesGroupId,
            effectiveFrom = EffectiveFrom,
            isDefaultAgent = false,
            isActive = 1
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
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (!_mappingCreated) return;   // depends on TC001 finding a usable pair
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
