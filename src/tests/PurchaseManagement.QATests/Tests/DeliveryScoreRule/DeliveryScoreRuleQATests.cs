namespace PurchaseManagement.QATests.Tests.DeliveryScoreRule;

// ─────────────────────────────────────────────────────────────────────────────
// DeliveryScoreRule — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — DeliveryScoreRuleController):
//   Route = [Route("api/[controller]")] → /api/DeliveryScoreRule
//   POST   /api/DeliveryScoreRule        { ruleCode?, description?, delayDaysFrom, delayDaysTo, score, sortOrder }
//   PUT    /api/DeliveryScoreRule         { id, ...(no ruleCode), isActive }
//   DELETE /api/DeliveryScoreRule/{id}    (id bound from ROUTE)
//   GET    /api/DeliveryScoreRule?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/DeliveryScoreRule/{id}    (always 200; data wraps ApiResponse)
//   GET    /api/DeliveryScoreRule/by-name?term=
//
// Key facts that shaped assertions:
//   • No FK — clean master.
//   • delayDaysFrom/To (int), score (decimal), sortOrder (int) are required numerics.
//   • All write endpoints return Ok(200) with isSuccess inside the body → negatives tolerated (200,400).
//   • Create returns 200 envelope; id captured via CreatedId().
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DeliveryScoreRuleCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DeliveryScoreRuleQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DeliveryScoreRule";

    private static string _createdCode = string.Empty;

    public DeliveryScoreRuleQATests(QAServerFixture fixture) => _f = fixture;

    private string NewCode() => _f.EntityCode[..10];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            ruleCode = _createdCode,
            description = "QA Test Delivery Score Rule",
            delayDaysFrom = 0,
            delayDaysTo = 5,
            score = 10.5m,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode)
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
            ruleCode = "NOAUTH01",
            description = "No Auth Rule",
            delayDaysFrom = 0,
            delayDaysTo = 5,
            score = 10m,
            sortOrder = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NegativeDelayDays_Returns200_Or_400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            ruleCode = NewCode(),
            description = "Bad delay",
            delayDaysFrom = -5,
            delayDaysTo = -1,
            score = 10m,
            sortOrder = 1
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmptyBody_Returns200_Or_400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
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

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (ruleCode immutable — not in update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Delivery Score Rule",
            delayDaysFrom = 1,
            delayDaysTo = 10,
            score = 20m,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
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
            description = "QA Updated Delivery Score Rule",
            delayDaysFrom = 1,
            delayDaysTo = 10,
            score = 20m,
            sortOrder = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Delivery Score Rule",
            delayDaysFrom = 1,
            delayDaysTo = 10,
            score = 20m,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Delivery Score Rule",
            delayDaysFrom = 1,
            delayDaysTo = 10,
            score = 20m,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id from ROUTE; controller always 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns200_Or_400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns200_Or_400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns200_Or_400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }
}
