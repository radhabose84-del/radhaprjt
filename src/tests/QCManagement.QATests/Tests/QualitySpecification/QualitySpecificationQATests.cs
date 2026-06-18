namespace QCManagement.QATests.Tests.QualitySpecification;

// ─────────────────────────────────────────────────────────────────────────────
// QualitySpecification — live-server QA suite (TRANSACTIONAL; create-happy + lifecycle SKIPPED).
//
// Contract verified against source (2026-06-16):
//   POST   /api/qc/QualitySpecification        { qualityTemplateId, applicableLevelId, qcTypeId,
//                                                itemCategoryId?/itemId?, effectiveFromDate,
//                                                effectiveToDate?, parameters:[ matching template ] }
//   PUT    /api/qc/QualitySpecification         { id, ..., isActive, parameters:[...] }
//   DELETE /api/qc/QualitySpecification?id={id} (id bound from QUERY, not route)
//   GET    /api/qc/QualitySpecification?PageNumber=&PageSize=&SearchTerm=&QualityTemplateId=
//                                       &ApplicableLevelId=&QcTypeId=&ItemCategoryId=&ItemId=&IsActive=
//   GET    /api/qc/QualitySpecification/{id}     (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/qc/QualitySpecification/by-name?term=
//
// Why create-happy + lifecycle are SKIPPED:
//   A valid spec requires a QualityTemplate + applicableLevel/qcType QC MiscMaster values +
//   cross-module Inventory item/category + nested params matching the template + effective dates —
//   none guaranteed on the QA clone. Attribute-level [Fact(Skip=...)] is explicit pending work.
//   GetAll smoke (tolerant 200/404), no-auth 401, empty-body 400, by-name reachability, and
//   GetById-nonexistent tolerant remain ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("QualitySpecificationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class QualitySpecificationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/qc/QualitySpecification";

    public QualitySpecificationQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED; negatives ACTIVE)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: QualityTemplate + applicableLevel/qcType QC misc + Inventory item/category + matching params"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            qualityTemplateId = 1,
            applicableLevelId = 1,
            qcTypeId = 1,
            itemId = _f.ActiveItemId,
            effectiveFromDate = today.ToString("yyyy-MM-dd"),
            parameters = new[]
            {
                new { qualityParameterId = 1, sequenceNo = 1, isMandatory = true }
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
            qualityTemplateId = 1,
            applicableLevelId = 1,
            qcTypeId = 1
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
    public async Task TC004_Create_MissingRequiredFields_Returns400()
    {
        // Only an effective date supplied — template/level/qcType/params missing.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            effectiveFromDate = "2026-01-01"
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
    public async Task TC030_GetById_NonExistentId_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
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

    [Fact(Skip = "needs seeded data: a created QualitySpecification id (TC001 blocked on template/misc/item seeds)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            qualityTemplateId = 1,
            applicableLevelId = 1,
            qcTypeId = 1,
            itemId = _f.ActiveItemId,
            effectiveFromDate = today.ToString("yyyy-MM-dd"),
            isActive = 1,
            parameters = new[] { new { qualityParameterId = 1, sequenceNo = 1, isMandatory = true } }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
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

    [Fact(Skip = "needs seeded data: a created QualitySpecification id (TC001 blocked on template/misc/item seeds)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
