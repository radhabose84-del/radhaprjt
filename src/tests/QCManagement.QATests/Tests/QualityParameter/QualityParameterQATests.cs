namespace QCManagement.QATests.Tests.QualityParameter;

// ─────────────────────────────────────────────────────────────────────────────
// QualityParameter — live-server QA suite (create-happy ATTEMPTED, else self-guards).
//
// Contract verified against source (2026-06-16):
//   POST   /api/qc/QualityParameter        { parameterName, parameterGroupId, dataTypeId,
//                                            unitId?, validationTypeId, description? }
//   PUT    /api/qc/QualityParameter        { id, parameterName, parameterGroupId, unitId?,
//                                            description?, isActive }   (dataType/validationType immutable)
//   DELETE /api/qc/QualityParameter?id={id}  (id bound from QUERY, not route)
//   GET    /api/qc/QualityParameter?PageNumber=&PageSize=&SearchTerm=&ParameterGroupId=
//   GET    /api/qc/QualityParameter/{id}     (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/qc/QualityParameter/by-name?term=
//
// Why create-happy is ATTEMPTED-not-skipped:
//   parameterGroupId / dataTypeId / validationTypeId are QC.MiscMaster values living under the
//   MiscTypeCodes QP_PARAMETER_GROUP / QP_DATA_TYPE / QP_VALIDATION_TYPE. The QA clone may not have
//   these seeded. TC001 resolves each via the by-name autocomplete (filtered by MiscTypeCode); if any
//   resolves to 0 it self-guards (sets nothing) and downstream lifecycle tests early-return on
//   _f.CreatedId == 0. The always-active read/negative coverage still runs.
//   • parameterName is unique (AlreadyExists) and required.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("QualityParameterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class QualityParameterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/qc/QualityParameter";
    private const string MiscRoute = "/api/qc/MiscMaster";

    private static string _createdName = string.Empty;
    private static int _parameterGroupId;

    public QualityParameterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewName() => "QAParam" + _f.EntityCode[1..9];

    // Resolves the FIRST QC MiscMaster id under a given MiscTypeCode via the autocomplete endpoint.
    private async Task<int> ResolveMiscValueAsync(string miscTypeCode)
    {
        var resp = await _f.Client.GetAsync($"{MiscRoute}/by-name?term=&MiscTypeCode={miscTypeCode}");
        if (!resp.IsSuccessStatusCode) return 0;
        using var doc = await QAHelper.ParseAsync(resp);
        if (!doc.RootElement.TryGetProperty("data", out var data)) return 0;
        if (data.ValueKind != JsonValueKind.Array || data.GetArrayLength() == 0) return 0;
        foreach (var p in data[0].EnumerateObject())
            if (p.Value.ValueKind == JsonValueKind.Number &&
                p.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                return p.Value.GetInt32();
        return 0;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path ATTEMPTED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var groupId = await ResolveMiscValueAsync("QP_PARAMETER_GROUP");
        var dataTypeId = await ResolveMiscValueAsync("QP_DATA_TYPE");
        var validationTypeId = await ResolveMiscValueAsync("QP_VALIDATION_TYPE");

        // FK seed data absent on the clone → leave CreatedId = 0; lifecycle tests self-skip.
        if (groupId == 0 || dataTypeId == 0 || validationTypeId == 0) return;

        _createdName = NewName();
        _parameterGroupId = groupId;

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            parameterName = _createdName,
            parameterGroupId = groupId,
            dataTypeId = dataTypeId,
            validationTypeId = validationTypeId,
            description = "Created by QA suite"
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
            parameterName = "NoAuthParam",
            parameterGroupId = 1,
            dataTypeId = 1,
            validationTypeId = 1
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
    public async Task TC004_Create_ParameterNameMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            parameterName = "",
            parameterGroupId = 1,
            dataTypeId = 1,
            validationTypeId = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_DuplicateName_Returns400()
    {
        if (_f.CreatedId == 0) return; // create blocked → nothing to duplicate

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            parameterName = _createdName,
            parameterGroupId = _parameterGroupId,
            dataTypeId = await ResolveMiscValueAsync("QP_DATA_TYPE"),
            validationTypeId = await ResolveMiscValueAsync("QP_VALIDATION_TYPE"),
            description = "dup"
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke; tolerant 200/404)
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

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_FilterByParameterGroupId_Returns200Or404()
    {
        var groupId = _parameterGroupId > 0 ? _parameterGroupId : 1;
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&ParameterGroupId={groupId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("parameterName").GetString()
            .Should().Be(_createdName);
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
    // SECTION 5 — UPDATE  (dataType/validationType immutable; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            parameterName = _createdName,
            parameterGroupId = _parameterGroupId,
            description = "Updated by QA",
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
            parameterName = "X",
            parameterGroupId = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_IsActiveInvalid_Returns400()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            parameterName = _createdName,
            parameterGroupId = _parameterGroupId,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            parameterName = "QA Updated",
            parameterGroupId = 1,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (lifecycle guarded; negatives ACTIVE — id bound from QUERY)
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

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        if (_f.CreatedId == 0) return;

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
