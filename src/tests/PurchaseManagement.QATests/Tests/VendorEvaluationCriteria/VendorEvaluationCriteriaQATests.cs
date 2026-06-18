namespace PurchaseManagement.QATests.Tests.VendorEvaluationCriteria;

// ─────────────────────────────────────────────────────────────────────────────
// VendorEvaluationCriteria — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (VendorEvaluationCriteriaController, 2026-06-17):
//   POST   /api/vendorevaluationcriteria
//          { criteriaCode, criteriaName, description?, weightagePercent, scoringMethodId,
//            minimumScore, ratingImpactId, sortOrder, calculationType? }  → 200 (ApiResponseDTO)
//   PUT    /api/vendorevaluationcriteria  { id, criteriaName, ...(no code), isActive } → 200
//   DELETE /api/vendorevaluationcriteria/{id}   (id bound from ROUTE)
//   GET    /api/vendorevaluationcriteria?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/vendorevaluationcriteria/{id}   (200 + data:null when not found — NO 404 guard)
//   GET    /api/vendorevaluationcriteria/by-name?term=
//
// Key facts:
//   • criteriaCode is required + immutable (excluded from update) + unique (duplicate → 400).
//   • scoringMethodId & ratingImpactId are required same-module MiscMaster FKs
//     (/api/purchase/miscmaster) — resolved at runtime; when either is 0 the create-happy
//     step + downstream id-dependent tests self-skip (guard on _f.CreatedId==0).
//   • Create returns 200 (ApiResponseDTO shape) — assert with AssertOkAsync.
//   • GetAll(Smoke) / no-auth / empty-body / required negatives stay ACTIVE regardless.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("VendorEvaluationCriteriaCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class VendorEvaluationCriteriaQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/vendorevaluationcriteria";
    private const string MiscMasterRoute = "/api/purchase/miscmaster";

    private static int _scoringMethodId;
    private static int _ratingImpactId;
    private static string _createdCode = string.Empty;

    public VendorEvaluationCriteriaQATests(QAServerFixture fixture) => _f = fixture;

    private string NewCode() => _f.EntityCode[..10];
    private string NewName() => $"QA Criteria {_f.EntityCode[..8]}";

    private async Task ResolveFksAsync()
    {
        if (_scoringMethodId == 0) _scoringMethodId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        if (_ratingImpactId == 0) _ratingImpactId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
    }

    private object BuildCreate(string code, string name) => new
    {
        criteriaCode = code,
        criteriaName = name,
        description = "Created by QA suite",
        weightagePercent = 10m,
        scoringMethodId = _scoringMethodId,
        minimumScore = 1m,
        ratingImpactId = _ratingImpactId,
        sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
        calculationType = "AVG"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId; 200 expected)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await ResolveFksAsync();
        if (_scoringMethodId == 0 || _ratingImpactId == 0)
            return; // self-skip: required MiscMaster FK not resolvable on the QA clone

        _createdCode = NewCode();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildCreate(_createdCode, NewName()));

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
            criteriaCode = "NOAUTH01",
            criteriaName = "No Auth Criteria",
            weightagePercent = 10m,
            scoringMethodId = 1,
            minimumScore = 1m,
            ratingImpactId = 1,
            sortOrder = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_CodeEmpty_Returns400()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildCreate("", NewName()));
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_NameEmpty_Returns400()
    {
        await ResolveFksAsync();
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildCreate(NewCode(), ""));
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_InvalidMiscMasterFk_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            criteriaCode = NewCode(),
            criteriaName = NewName(),
            weightagePercent = 10m,
            scoringMethodId = 999999,
            minimumScore = 1m,
            ratingImpactId = 999999,
            sortOrder = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_DuplicateCode_Returns400()
    {
        if (_f.CreatedId == 0) return; // self-skip: no created row to duplicate
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, BuildCreate(_createdCode, NewName()));
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (Smoke)
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

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId == 0) return; // self-skip: no created row
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
    // SECTION 5 — UPDATE  (code is immutable — not in update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return; // self-skip: no created row
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            criteriaName = NewName() + " Upd",
            description = "Updated by QA",
            weightagePercent = 15m,
            scoringMethodId = _scoringMethodId,
            minimumScore = 2m,
            ratingImpactId = _ratingImpactId,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            calculationType = "AVG",
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            criteriaName = "QA Upd",
            weightagePercent = 10m,
            scoringMethodId = 1,
            minimumScore = 1m,
            ratingImpactId = 1,
            sortOrder = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NonExistentId_Returns400_NotFound()
    {
        if (_scoringMethodId == 0 || _ratingImpactId == 0) return; // self-skip: FK unresolvable
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            criteriaName = NewName(),
            weightagePercent = 10m,
            scoringMethodId = _scoringMethodId,
            minimumScore = 1m,
            ratingImpactId = _ratingImpactId,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode),
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from ROUTE: /{id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
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
        if (_f.CreatedId == 0) return; // self-skip: no created row
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
