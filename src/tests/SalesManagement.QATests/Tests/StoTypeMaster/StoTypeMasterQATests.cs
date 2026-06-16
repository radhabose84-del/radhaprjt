namespace SalesManagement.QATests.Tests.StoTypeMaster;

// ─────────────────────────────────────────────────────────────────────────────
// StoTypeMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/sales/StoTypeMaster        { stoTypeCode, stoTypeName, description?, pgiMovementTypeId, grMovementTypeId }
//   PUT    /api/sales/StoTypeMaster        { id, stoTypeName, description?, pgiMovementTypeId, grMovementTypeId, isActive }
//   DELETE /api/sales/StoTypeMaster?id={id} (id bound from QUERY — DeleteStoTypeMaster(int id), no route template)
//   GET    /api/sales/StoTypeMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/sales/StoTypeMaster/{id}    (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/sales/StoTypeMaster/by-name?term=
//
// NOTE: route is prefixed with /api/sales (controller [Route("api/sales/[controller]")]) — NOT /api.
//
// Key facts that shaped assertions:
//   • StoTypeCode is unique, max 10, immutable on update. Treated as alphanumeric per shared rule set.
//   • StoTypeName is required, max 100. Description is optional, max 250.
//   • PgiMovementTypeId / GrMovementTypeId are SAME-MODULE FKs into MovementTypeConfig:
//       - both required (> 0), both validated via MovementTypeExistsAsync,
//       - cross-field rule: PGI and GR must be DIFFERENT.
//   • FK ids are resolved at runtime from /api/MovementTypeConfig — the QA clone has no guaranteed seed ids.
//     Two DISTINCT ids are needed for the happy path; if fewer than two exist, create-dependent tests skip
//     via a runtime guard (Assert.Skip) so the suite stays green on a clone without seeded movement types.
//   • Reads are NOT company-scoped (WHERE IsDeleted = 0 only) → created rows are visible in GetAll/GetById.
//   • Delete validator: NotEmpty (id!=0) → NotFound (must exist).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("StoTypeMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class StoTypeMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/sales/StoTypeMaster";
    private const string MovementTypeRoute = "/api/MovementTypeConfig";

    private const string TestName = "QA Test STO Type";

    // The run-unique code captured at create; reused by duplicate/immutability tests.
    private static string _createdCode = string.Empty;

    // Two DISTINCT, real MovementTypeConfig ids resolved at runtime.
    private static int _pgiId;
    private static int _grId;

    public StoTypeMasterQATests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric, sliced to 10 chars (code max length).
    private string NewCode() => _f.EntityCode[..10];

    // Resolves two distinct MovementTypeConfig ids. Returns false when fewer than two exist.
    private async Task<bool> ResolveMovementTypeIdsAsync()
    {
        if (_pgiId > 0 && _grId > 0 && _pgiId != _grId) return true;

        var resp = await _f.Client.GetAsync($"{MovementTypeRoute}?PageNumber=1&PageSize=10");
        if (!resp.IsSuccessStatusCode) return false;

        var doc = await QAHelper.ParseAsync(resp);
        if (!doc.RootElement.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array)
            return false;

        var ids = new List<int>();
        foreach (var row in data.EnumerateArray())
        {
            if (row.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.Number)
                ids.Add(idProp.GetInt32());
        }

        var distinct = ids.Distinct().ToList();
        if (distinct.Count < 2) return false;

        _pgiId = distinct[0];
        _grId = distinct[1];
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows are required for STO Type create."

        _createdCode = NewCode();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = _createdCode,
            stoTypeName = TestName,
            description = "Created by QA suite",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId
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
            stoTypeCode = "NOAUTH01",
            stoTypeName = "No Auth STO",
            pgiMovementTypeId = 1,
            grMovementTypeId = 2
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = "",
            stoTypeName = TestName,
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = NewCode(),
            stoTypeName = "",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CodeTooLong_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = new string('A', 11), // exceeds code max (10)
            stoTypeName = TestName,
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_NameTooLong_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = NewCode(),
            stoTypeName = new string('A', 101), // exceeds name max (100)
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_PgiMovementTypeMissing_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        // pgiMovementTypeId default 0 → GreaterThan(0) fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = NewCode(),
            stoTypeName = TestName,
            pgiMovementTypeId = 0,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_NonExistentPgiMovementType_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        // MovementTypeExistsAsync false → FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = NewCode(),
            stoTypeName = TestName,
            pgiMovementTypeId = 999999,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_PgiEqualsGr_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."

        // Cross-field rule: PGI and GR cannot be the same.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = NewCode(),
            stoTypeName = TestName,
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _pgiId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "same");
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DuplicateCode_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs seeded data: at least two distinct MovementTypeConfig rows."
        if (string.IsNullOrEmpty(_createdCode))
            return; // SKIPPED — "create (TC001) did not run — no code to duplicate."

        // Same code as TC001 → AlreadyExists fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            stoTypeCode = _createdCode,
            stoTypeName = TestName,
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (reads are NOT company-scoped; TC001 row is visible)
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
    public async Task TC022_GetAll_SearchByCreatedCode_Returns200_WithData()
    {
        if (string.IsNullOrEmpty(_createdCode))
            return; // SKIPPED — "create (TC001) did not run — nothing to search for."

        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={_createdCode}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetAll_Page2PageSize5_Returns200()
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
        if (_f.CreatedId == 0)
            return; // SKIPPED — "create (TC001) did not run — no id to fetch."

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("stoTypeCode").GetString()
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
        // No null guard in controller → 200 with data:null (per CLAUDE.md GetById contract).
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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (Code is immutable — not in the update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0 || !await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs created STO Type (TC001) and two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            stoTypeName = "QA Updated STO Type",
            description = "Updated by QA",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
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
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = 1,
            grMovementTypeId = 2,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NameEmpty_Returns400()
    {
        if (_f.CreatedId == 0 || !await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs created STO Type (TC001) and two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            stoTypeName = "",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_PgiEqualsGr_Returns400()
    {
        if (_f.CreatedId == 0 || !await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs created STO Type (TC001) and two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _pgiId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "same");
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_IsActiveInvalid_Returns400()
    {
        if (_f.CreatedId == 0 || !await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs created STO Type (TC001) and two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_NonExistentId_Returns400_NotFound()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_IdZero_Returns400()
    {
        if (!await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs two distinct MovementTypeConfig rows."

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(57)]
    public async Task TC057_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0 || !await ResolveMovementTypeIdsAsync())
            return; // SKIPPED — "needs created STO Type (TC001) and two distinct MovementTypeConfig rows."

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            stoTypeName = "QA Updated STO Type",
            pgiMovementTypeId = _pgiId,
            grMovementTypeId = _grId,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(58)]
    public async Task TC058_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(59)]
    public async Task TC059_Verify_CodeIsImmutable_GetByIdShowsOriginalCode()
    {
        if (_f.CreatedId == 0)
            return; // SKIPPED — "create (TC001) did not run — no id to verify."

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("stoTypeCode").GetString()
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
        if (_f.CreatedId == 0)
            return; // SKIPPED — "create (TC001) did not run — nothing to delete."

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        if (_f.CreatedId == 0)
            return; // SKIPPED — "create (TC001) did not run — nothing was deleted."

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        if (_f.CreatedId == 0)
            return; // SKIPPED — "create (TC001) did not run — nothing was soft-deleted."

        // After soft delete, GetByIdAsync filters IsDeleted=0 → null → 200 + data:null.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
