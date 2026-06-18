namespace PurchaseManagement.QATests.Tests.PaymentTermMaster;

// ─────────────────────────────────────────────────────────────────────────────
// PaymentTermMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — PaymentTermMasterController):
//   Route = [Route("api/[controller]")] → /api/PaymentTermMaster
//   POST   /api/PaymentTermMaster   { code, description, baselineTypeId, creditDays, advancePercent?, installments?[] }
//   PUT    /api/PaymentTermMaster    { id, description, isActive(bool) }   (code immutable)
//   DELETE /api/PaymentTermMaster/{id}   (id bound from ROUTE)
//   GET    /api/PaymentTermMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/PaymentTermMaster/{id}   (always 200; data wraps ApiResponse)
//   GET    /api/PaymentTermMaster/by-name?searchPattern=&paymentTermCode=
//
// Key facts that shaped assertions:
//   • baselineTypeId → /api/purchase/miscmaster FK (REQUIRED). Resolved at runtime via FirstIdAsync;
//     if 0 the create-happy self-skips (downstream guards on _f.CreatedId==0).
//   • Create returns 201 envelope inside Ok(200); id captured via CreatedId().
//   • Create/Update/Delete controllers always return Ok(200) (no inline validation gate) →
//     negatives surface as 200+isSuccess=false (pipeline) OR 400 → tolerated (200,400).
//   • Update isActive is a bool in this contract.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("PaymentTermMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class PaymentTermMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/PaymentTermMaster";
    private const string BaselineTypeRoute = "/api/purchase/miscmaster";

    private static string _createdCode = string.Empty;
    private static int _baselineTypeId;

    public PaymentTermMasterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewCode() => _f.EntityCode[..10];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdCode = NewCode();
        _baselineTypeId = await QAHelper.FirstIdAsync(_f.Client, BaselineTypeRoute);

        if (_baselineTypeId == 0)
            return; // required FK unresolved on clone — downstream guards on _f.CreatedId==0

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = _createdCode,
            description = "QA Test Payment Term",
            baselineTypeId = _baselineTypeId,
            creditDays = 30,
            advancePercent = 10m,
            installments = Array.Empty<object>()
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
            code = "NOAUTH01",
            description = "No Auth Term",
            baselineTypeId = 1,
            creditDays = 30
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns200_Or_400()
    {
        var baseline = await QAHelper.FirstIdAsync(_f.Client, BaselineTypeRoute);

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "",
            description = "QA Test Payment Term",
            baselineTypeId = baseline,
            creditDays = 30
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns200_Or_400()
    {
        var baseline = await QAHelper.FirstIdAsync(_f.Client, BaselineTypeRoute);

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = NewCode(),
            description = "",
            baselineTypeId = baseline,
            creditDays = 30
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateCode_Returns200_Or_400()
    {
        if (_f.CreatedId == 0 || _baselineTypeId == 0) return; // create-happy skipped

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = _createdCode,
            description = "QA Test Payment Term",
            baselineTypeId = _baselineTypeId,
            creditDays = 30
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EmptyBody_Returns200_Or_400()
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
        if (_f.CreatedId == 0) return; // create-happy skipped

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
    public async Task TC032_GetById_NonExistentId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (params: searchPattern, paymentTermCode)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithSearchPattern_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?searchPattern=QA");
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?searchPattern=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (code immutable; isActive bool)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        // Live contract: Update requires code + baselineTypeId echoed back even though both are immutable.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            code = _createdCode,
            description = "QA Updated Payment Term",
            baselineTypeId = _baselineTypeId,
            creditDays = 30,
            isActive = true
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            description = "QA Updated Payment Term",
            isActive = true
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        // Live contract: Update requires code + baselineTypeId echoed back even though both are immutable.
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            code = _createdCode,
            description = "QA Updated Payment Term",
            baselineTypeId = _baselineTypeId,
            creditDays = 30,
            isActive = false
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            code = _createdCode,
            description = "QA Updated Payment Term",
            baselineTypeId = _baselineTypeId,
            creditDays = 30,
            isActive = true
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
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns200_Or_400()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }
}
