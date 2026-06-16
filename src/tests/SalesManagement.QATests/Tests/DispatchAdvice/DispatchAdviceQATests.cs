namespace SalesManagement.QATests.Tests.DispatchAdvice;

// ─────────────────────────────────────────────────────────────────────────────
// DispatchAdvice — live-server QA suite (TRANSACTIONAL entity). NO UPDATE endpoint.
//
// Contract verified against source (2026-06-15):
//   GET    /api/DispatchAdvice?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/DispatchAdvice/by-name?term=
//   GET    /api/DispatchAdvice/{id}                       (200 + data:null when absent — no 404 guard)
//   GET    /api/DispatchAdvice/stock?itemId=&lotId=
//   GET    /api/DispatchAdvice/validate-packno?itemId=&lotId=&startPackNo=&endPackNo=&packTypeId=
//   GET    /api/DispatchAdvice/pack-range?itemId=&lotId=&packTypeId=&range=&...
//   GET    /api/DispatchAdvice/packing-list/{dispatchAdviceId}
//   POST   /api/DispatchAdvice/packing-list/multiple      [FromBody] List<int> dispatchAdviceIds
//   POST   /api/DispatchAdvice                            { dispatchDate, salesOrderId, partyId,
//                                                            dispatchTypeId, unitId, totOrderQty,
//                                                            totDispatchedQty, totPendingQty,
//                                                            details:[{ salesOrderDetailId, itemId,
//                                                                       lotId, startPackNo, endPackNo }] }
//   DELETE /api/DispatchAdvice?id={id}                    (id bound from QUERY — simple int action param)
//
//   NO PUT/Update endpoint exists → no Update section.
//   Create command implements IRequirePermission (CanAdd).
//
// Key facts that shaped assertions:
//   • Create requires a seeded SalesOrder + detail, a Party, and packed stock (lot/pack
//     ranges in details). The QA clone has no guaranteed seeded sales order + packed stock
//     → create-happy and delete-happy are SKIPPED.
//   • Reads (stock, validate-packno, pack-range, packing-list, packing-list/multiple) are
//     reachability-tested (200/400/404 tolerant) — they may NRE/500 on empty data; the
//     contract is "endpoint exists & is auth-gated".
//   • All negatives + smoke (GetAll) remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DispatchAdviceCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DispatchAdviceQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DispatchAdvice";

    public DispatchAdviceQATests(QAServerFixture fixture) => _f = fixture;

    private static string Today() => DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a posted SalesOrder + detail, a Party, and packed stock (lot/pack ranges); QA clone has no guaranteed sales order + packed stock"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchDate = Today(),
            salesOrderId = 1,
            partyId = 1,
            dispatchTypeId = 1,
            unitId = 1,
            totOrderQty = 10m,
            totDispatchedQty = 10m,
            totPendingQty = 0m,
            distance = 0m,
            details = new[]
            {
                new { salesOrderDetailId = 1, itemId = 1, lotId = 1, startPackNo = 1, endPackNo = 1 }
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
            dispatchDate = Today(),
            salesOrderId = 1,
            partyId = 1,
            dispatchTypeId = 1,
            unitId = 1
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
    public async Task TC004_Create_MissingRequired_Returns400()
    {
        // Missing salesOrderId / partyId / dispatchTypeId / details → required validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dispatchDate = Today(),
            salesOrderId = 0,
            partyId = 0,
            dispatchTypeId = 0,
            unitId = 0
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke)
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
    // SECTION 3 — GET BY ID + EXTRA READS  (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_Stock_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/stock?itemId=999999&lotId=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_ValidatePackNo_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/validate-packno?itemId=999999&lotId=999999&startPackNo=1&endPackNo=1&packTypeId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_PackRange_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync(
            $"{BaseRoute}/pack-range?itemId=999999&lotId=999999&packTypeId=1&range=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_PackingList_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/packing-list/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_PackingListMultiple_Reachable_Returns200Or400Or404()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/packing-list/multiple", new[] { 999999 });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_Stock_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/stock?itemId=1&lotId=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
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
    // SECTION 5 — DELETE  (id bound from QUERY: ?id={id}; happy BLOCKED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns200Or400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact(Skip = "needs seeded data: depends on TC001 create which is blocked"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
