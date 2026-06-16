namespace SalesManagement.QATests.Tests.MarketingOfficer;

// ─────────────────────────────────────────────────────────────────────────────
// MarketingOfficer — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/MarketingOfficer
//          { employeeNo, employeeName, mobileNo?, email?, unit, department,
//            designation, salesOfficeId, salesGroups: [ { salesGroupId } ] }
//   PUT    /api/MarketingOfficer
//          { id, employeeName, mobileNo?, email?, unit, department,
//            designation, salesOfficeId, isActive, salesGroups: [ { salesGroupId } ] }
//          (employeeNo is NOT in the update command → immutable on update)
//   DELETE /api/MarketingOfficer?id={id}    (id bound from QUERY — primitive on [ApiController])
//   GET    /api/MarketingOfficer?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/MarketingOfficer/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/MarketingOfficer/by-name?term=
//   GET    /api/MarketingOfficer/employee-lookup?empNo=   (reachability only)
//
// Key facts that shaped assertions (from Create/Update validators):
//   • EmployeeNo: NotEmpty + Alphanumeric (^[A-Za-z0-9]+$, no spaces) + AlreadyExists (unique). Immutable.
//   • EmployeeName / Unit / Department / Designation: NotEmpty + MaxLength.
//   • MobileNo (optional): must match ^[6-9]\d{9}$ when present.
//   • Email (optional): must be a valid email when present.
//   • SalesOfficeId: same-module FK (SalesOfficeExistsAsync) — resolved at runtime via /api/SalesOffice.
//   • SalesGroups: NotNull + at least one element + no duplicate ids + all ids active/non-deleted.
//     salesGroupId resolved at runtime via /api/SalesGroup (fallback 1).
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist) → SoftDelete (no dependents).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("MarketingOfficerCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class MarketingOfficerQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/MarketingOfficer";

    private const string TestName = "QA Test Marketing Officer";

    // Run-unique alphanumeric employee no captured at create; reused by duplicate/immutability tests.
    private static string _createdCode = string.Empty;

    // FK ids resolved once (per collection) at first create.
    private static int _salesOfficeId;
    private static int _salesGroupId;

    public MarketingOfficerQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric, sliced to 10 chars (Q + reversed-tick digits).
    private string NewCode() => _f.EntityCode[..10];

    private async Task EnsureFkIdsAsync()
    {
        if (_salesOfficeId == 0)
            _salesOfficeId = await ResolveOrFallback("/api/SalesOffice");
        if (_salesGroupId == 0)
            _salesGroupId = await ResolveOrFallback("/api/SalesGroup");
    }

    private async Task<int> ResolveOrFallback(string route)
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, route);
        return id > 0 ? id : 1;
    }

    private object BuildCreateBody(string code) => new
    {
        employeeNo = code,
        employeeName = TestName,
        mobileNo = "9876543210",
        email = "qa.officer@example.com",
        unit = "QA Unit",
        department = "QA Department",
        designation = "QA Designation",
        salesOfficeId = _salesOfficeId,
        salesGroups = new[] { new { salesGroupId = _salesGroupId } }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await EnsureFkIdsAsync();
        _createdCode = NewCode();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildCreateBody(_createdCode));

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
            employeeNo = "NOAUTH01",
            employeeName = TestName,
            unit = "U",
            department = "D",
            designation = "Des",
            salesOfficeId = 1,
            salesGroups = new[] { new { salesGroupId = 1 } }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmployeeNoEmpty_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = "",
            employeeName = TestName,
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EmployeeNameEmpty_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = "",
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_RequiredTextFieldsEmpty_Returns400()
    {
        // Unit / Department / Designation are all NotEmpty.
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = TestName,
            unit = "",
            department = "",
            designation = "",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_EmployeeNoWithSpace_Returns400()
    {
        // Alphanumeric pattern ^[A-Za-z0-9]+$ — spaces are not allowed.
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = "QA EMP",
            employeeName = TestName,
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_EmployeeNameTooLong_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = new string('A', 201),
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_InvalidMobile_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = TestName,
            mobileNo = "12345",
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_InvalidEmail_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = TestName,
            email = "not-an-email",
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_NoSalesGroups_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = TestName,
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = _salesOfficeId,
            salesGroups = Array.Empty<object>()
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "sales group");
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_NonExistentSalesOffice_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            employeeNo = NewCode(),
            employeeName = TestName,
            unit = "QA Unit",
            department = "QA Department",
            designation = "QA Designation",
            salesOfficeId = 999999,
            salesGroups = new[] { new { salesGroupId = _salesGroupId } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_Create_DuplicateEmployeeNo_Returns400()
    {
        // Same employeeNo as TC001 → AlreadyExists fails.
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildCreateBody(_createdCode));

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(13)]
    public async Task TC013_Create_EmptyBody_Returns400()
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
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out var data).Should().BeTrue();
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200_WithCorrectCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("employeeNo").GetString()
            .Should().Be(_createdCode);
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
    // SECTION 4 — AUTOCOMPLETE  (param is `term`) + employee-lookup reachability
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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(43)]
    public async Task TC043_EmployeeLookup_IsReachable_Returns200()
    {
        // Reachability only — depends on OldUnitId claim + external employee source.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/employee-lookup");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (EmployeeNo is immutable — not in the update command)
    // ─────────────────────────────────────────────────────────────────────────

    private object BuildUpdateBody(int id, int isActive, string name = "QA Updated Officer") => new
    {
        id,
        employeeName = name,
        mobileNo = "9876543210",
        email = "qa.officer@example.com",
        unit = "QA Unit",
        department = "QA Department",
        designation = "QA Designation",
        salesOfficeId = _salesOfficeId,
        isActive,
        salesGroups = new[] { new { salesGroupId = _salesGroupId } }
    };

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildUpdateBody(_f.CreatedId, 1));
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, BuildUpdateBody(_f.CreatedId, 1));
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NameEmpty_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildUpdateBody(_f.CreatedId, 1, name: ""));
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_NonExistentId_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildUpdateBody(999999, 1));
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_IdZero_Returns400()
    {
        await EnsureFkIdsAsync();
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, BuildUpdateBody(0, 1));
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_Inactivate_Then_Reactivate_Returns200()
    {
        await EnsureFkIdsAsync();

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, BuildUpdateBody(_f.CreatedId, 0));
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, BuildUpdateBody(_f.CreatedId, 1));
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(57)]
    public async Task TC057_Verify_EmployeeNoIsImmutable_GetByIdShowsOriginalCode()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("employeeNo").GetString()
            .Should().Be(_createdCode);
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
    public async Task TC091_Delete_NonExistentId_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        await QAHelper.Assert400Async(resp);
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
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        // After soft delete, GetByIdAsync filters IsDeleted=0 → null → 200 + data:null.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
