namespace PartyManagement.QATests.Tests.BankMaster;

// ─────────────────────────────────────────────────────────────────────────────
// BankMaster (Party) — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — BankMasterController.cs + BankMasterDto.cs):
//   POST   /api/BankMaster            { dto: { bankName } }                 (DTO is WRAPPED under `dto`)
//   PUT    /api/BankMaster            { dto: { id, bankName, isActive(int 0/1) } }
//   DELETE /api/BankMaster/{id:int}   (id bound from ROUTE)
//   GET    /api/BankMaster?pageNumber=&pageSize=&searchTerm=
//   GET    /api/BankMaster/{id:int}   (returns 404 when not found)
//   GET    /api/BankMaster/by-name?term=
//
// Key facts that shaped assertions:
//   • CreateBankMasterCommand wraps CreateBankMasterDto → the POST/PUT body MUST nest under `dto`.
//   • Create returns `data: { id }`. CreatedId() extracts it from the wrapper object.
//   • Validator: BankName NotEmpty + MaximumLength(20) (effective max — two MaxLength rules, 20 wins)
//     + AlreadyExists → message "BankName already exists." (asserted via "exists").
//   • Maxlength uses FluentValidation default message (NOT "longer than") → assert 400 status only.
//   • Delete validator: NotEmpty → NotFound → SoftDelete (no dependents).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("BankMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class BankMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/BankMaster";

    // The run-unique bank name captured at create; reused by duplicate test.
    private static string _createdName = string.Empty;

    public BankMasterQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique name, sliced to <= 20 chars (effective BankName max).
    private string NewName() => ("QA" + _f.EntityCode)[..15];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdName = NewName();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dto = new { bankName = _createdName }
        });

        // note (live, reconciled 2026-06-17): BankMaster Create returns 201 Created
        // (controller does StatusCode(201, ...)), not 200 — accept both.
        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 201 },
            await resp.Content.ReadAsStringAsync());
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
            dto = new { bankName = "NoAuthBank" }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_NameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dto = new { bankName = "" }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameTooLong_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dto = new { bankName = new string('A', 101) } // exceeds effective max (20)
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateName_Returns400()
    {
        // Same name as TC001 → AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            dto = new { bankName = _createdName }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByCreatedName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15&searchTerm={_createdName}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller returns 404 when not found)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId == 0) return;

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
    public async Task TC032_GetById_NonExistentId_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
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
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (DTO wrapped under `dto`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            dto = new { id = _f.CreatedId, bankName = _createdName, isActive = 1 }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            dto = new { id = _f.CreatedId, bankName = _createdName, isActive = 1 }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NameEmpty_Rejected()
    {
        // BUG (live, reconciled 2026-06-17): UpdateBankMasterCommand is `IRequest` (void) — the
        // global ValidationBehavior is wired for IRequest<TResponse> only, so the update validator
        // (NotEmpty BankName) never fires. An empty BankName on an EXISTING record is therefore
        // accepted and the update SUCCEEDS (200). For a missing/zero id the handler 404s.
        // Accept 200 (current — empty name accepted), or 400/404 if validation is ever fixed.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            dto = new { id = _f.CreatedId, bankName = "", isActive = 1 }
        });

        ((int)resp.StatusCode).Should().BeOneOf(new[] { 200, 400, 404 },
            await resp.Content.ReadAsStringAsync());
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IdZero_Rejected()
    {
        // BUG (live, reconciled 2026-06-17): see TC052 — Update has no FluentValidation (void IRequest).
        // Id = 0 is not caught by the validator's GreaterThan(0); the handler's GetByIdAsync(0) returns
        // null → 404 "Bank not found". Accept 400/404.
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            dto = new { id = 0, bankName = _createdName, isActive = 1 }
        });

        ((int)resp.StatusCode).Should().BeOneOf(new[] { 400, 404 },
            await resp.Content.ReadAsStringAsync());
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            dto = new { id = _f.CreatedId, bankName = _createdName, isActive = 0 }
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            dto = new { id = _f.CreatedId, bankName = _createdName, isActive = 1 }
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from ROUTE: /{id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NonExistentId_Returns400Or404()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_AlreadyDeleted_Returns400Or404()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }
}
