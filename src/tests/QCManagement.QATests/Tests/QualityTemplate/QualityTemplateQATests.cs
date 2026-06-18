namespace QCManagement.QATests.Tests.QualityTemplate;

// ─────────────────────────────────────────────────────────────────────────────
// QualityTemplate — live-server QA suite (create-happy SKIPPED; reads/negatives ACTIVE).
//
// Contract verified against source (2026-06-16):
//   POST   /api/qc/QualityTemplate         { templateName, description?,
//                                            parameters:[{ qualityParameterId, sequenceNo, isMandatory,
//                                              isCritical, inspectionMethodId?, sampleSize?, sampleUomId?,
//                                              isGradeApplicable, remarks? }] (>=1, no dup paramId) }
//   PUT    /api/qc/QualityTemplate          { id, templateName, description?, isActive, parameters:[...] }
//   DELETE /api/qc/QualityTemplate?id={id}  (id bound from QUERY, not route)
//   GET    /api/qc/QualityTemplate?PageNumber=&PageSize=&SearchTerm=&IsActive=
//   GET    /api/qc/QualityTemplate/{id}      (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/qc/QualityTemplate/by-name?term=
//
// Auto-code: TemplateCode QT-000001 generated server-side (templateName is the unique field).
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid template needs >=1 active QualityParameter id — itself requiring the QC MiscMaster
//   seed values (QP_PARAMETER_GROUP/QP_DATA_TYPE/QP_VALIDATION_TYPE) which the clone may not have.
//   These are attribute-level [Fact(Skip=...)] explicit pending work. GetAll smoke, no-auth,
//   empty-body / missing-required 400, and by-name reachability remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("QualityTemplateCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class QualityTemplateQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/qc/QualityTemplate";

    public QualityTemplateQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: an active QualityParameter (itself needs QC misc values QP_PARAMETER_GROUP/QP_DATA_TYPE/QP_VALIDATION_TYPE)"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var paramId = await QAHelper.FirstIdAsync(_f.Client, "/api/qc/QualityParameter");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            templateName = "QATemplate" + _f.EntityCode[1..9],
            description = "Created by QA suite",
            parameters = new[]
            {
                new
                {
                    qualityParameterId = paramId,
                    sequenceNo = 1,
                    isMandatory = true,
                    isCritical = false,
                    isGradeApplicable = false
                }
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
            templateName = "NoAuthTemplate",
            parameters = new[] { new { qualityParameterId = 1, sequenceNo = 1 } }
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
    public async Task TC004_Create_MissingTemplateName_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            templateName = "",
            parameters = new[] { new { qualityParameterId = 1, sequenceNo = 1 } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_NoParameters_Returns400()
    {
        // parameters must contain >= 1 line.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            templateName = "QANoParams" + _f.EntityCode[1..9],
            description = "no params",
            parameters = Array.Empty<object>()
        });

        await QAHelper.Assert400Async(resp);
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
    public async Task TC022_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
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
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (lifecycle BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a created QualityTemplate id (TC001 blocked on QualityParameter seed)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            templateName = "QATemplate" + _f.EntityCode[1..9],
            description = "Updated by QA",
            isActive = 1,
            parameters = new[]
            {
                new { qualityParameterId = 1, sequenceNo = 1, isMandatory = true, isCritical = false, isGradeApplicable = false }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            templateName = "X",
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (lifecycle BLOCKED; negatives ACTIVE — id bound from QUERY)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400Or404()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(400, 404);
    }

    [Fact(Skip = "needs seeded data: a created QualityTemplate id (TC001 blocked on QualityParameter seed)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
