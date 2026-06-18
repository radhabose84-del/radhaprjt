namespace PurchaseManagement.QATests.Tests.ReturnType;

// ─────────────────────────────────────────────────────────────────────────────
// ReturnType — live-server QA suite (CRUD lifecycle + negatives).  CLEAN master (FKs optional).
//
// Contract verified against source (2026-06-17 — ReturnTypeController):
//   Route = [Route("api/[controller]")] → /api/ReturnType
//   POST   /api/ReturnType   { code, description, inventoryImpactId?, financeImpactId?,
//                            isReplacementApplicable, isQcMandatory, approvalRoleCode? }   (inline validator → real 400)
//   PUT    /api/ReturnType    { id, description, ...(no code), isActive(int) }   (code immutable; inline 400)
//   DELETE /api/ReturnType/{id}   (id bound from ROUTE; inline delete validator → real 400)
//   GET    /api/ReturnType?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/ReturnType/{id}   (always 200; data wraps ApiResponse)
//   GET    /api/ReturnType/by-name?term=
//
// Key facts that shaped assertions:
//   • No required FK — fully self-contained. Create returns 201 envelope inside Ok(200) → CreatedId().
//   • Create/Update/Delete run validators inline → real 400 on rule failure.
//   • code is REQUIRED + immutable + unique.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ReturnTypeCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ReturnTypeQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ReturnType";

    private static string _createdCode = string.Empty;

    public ReturnTypeQATests(QAServerFixture fixture) => _f = fixture;

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
            code = _createdCode,
            description = "QA Test Return Type",
            isReplacementApplicable = false,
            isQcMandatory = false
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
            description = "No Auth Return Type",
            isReplacementApplicable = false,
            isQcMandatory = false
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = "",
            description = "QA Test Return Type",
            isReplacementApplicable = false,
            isQcMandatory = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DescriptionEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = NewCode(),
            description = "",
            isReplacementApplicable = false,
            isQcMandatory = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code = _createdCode,
            description = "QA Test Return Type",
            isReplacementApplicable = false,
            isQcMandatory = false
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EmptyBody_Returns400()
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
    // SECTION 5 — UPDATE  (code immutable; isActive int)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Return Type",
            isReplacementApplicable = true,
            isQcMandatory = true,
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
            description = "QA Updated Return Type",
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NonExistentId_Returns400_Or_200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            description = "QA Updated Return Type",
            isActive = 1
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Return Type",
            isReplacementApplicable = true,
            isQcMandatory = true,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            description = "QA Updated Return Type",
            isReplacementApplicable = true,
            isQcMandatory = true,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id from ROUTE; inline delete validator → real 400)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        await QAHelper.Assert400Async(resp);
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
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
    }
}
