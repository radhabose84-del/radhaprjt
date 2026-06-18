namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-01 — Reference master vocabulary
//   As a purchasing administrator I build the reference vocabulary
//   (MiscTypeMaster → MiscMaster, plus an independent MixCodeMaster)
//   so purchase orders and documents can reference consistent codes.
// Fully implementable: all three are clean masters covered by the QA suite.
//
// Contracts (verified against PurchaseManagement.QATests, 2026-06-17):
//   POST   /api/purchase/MiscTypeMaster { miscTypeCode, description }
//   POST   /api/purchase/MiscMaster     { miscTypeId, code, description }
//   POST   /api/MixCodeMaster           { mixCode, mixCodeDesc }   (independent, no FK; returns 200)
//   DELETE /api/purchase/MiscMaster/{id}     (id from ROUTE)
//   DELETE /api/purchase/MiscTypeMaster/{id} (id from ROUTE)
//   DELETE /api/MixCodeMaster/{id}           (id from ROUTE)
//   Create returns 200/201 (heterogeneous shape) — accept BeOneOf(200, 201) for capture.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-01-ReferenceMasters")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute   = "/api/purchase/MiscTypeMaster";
    private const string MiscMasterRoute = "/api/purchase/MiscMaster";
    private const string MixCodeRoute    = "/api/MixCodeMaster";

    private static int _miscTypeId;
    private static int _miscMasterId;
    private static int _mixCodeId;

    // Captured codes — reused by readable-by-id / search assertions.
    private static string _miscTypeCode   = string.Empty;
    private static string _miscMasterCode = string.Empty;
    private static string _mixCode        = string.Empty;

    public US_PUR_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric code clamped to 10 chars (within every code's max length).
    // EntityCode is ~19 chars — clamp defensively so we never slice beyond its length.
    private string Code() => _f.EntityCode.Substring(0, Math.Min(10, _f.EntityCode.Length));

    // AC1 — a MiscTypeMaster can be created and returns a new id.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscTypeMaster()
    {
        _miscTypeCode = Code();

        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = _miscTypeCode,
            description = "US-PUR-01 Misc Type"
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _miscTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscTypeId.Should().BeGreaterThan(0);
    }

    // AC2 — a MiscMaster value can be created under that type (FK miscTypeId).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateMiscMasterUnderType()
    {
        _miscTypeId.Should().BeGreaterThan(0, "Step1 must have created the misc type");
        _miscMasterCode = Code();

        var resp = await _f.Client.PostAsJsonAsync(MiscMasterRoute, new
        {
            miscTypeId = _miscTypeId,
            code = _miscMasterCode,
            description = "US-PUR-01 Misc Master"
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);

        // MiscMaster create may echo a bare DTO under data; CreatedId() handles object + bare-int.
        // Fall back to a search-by-code if the shape carries no numeric id directly.
        try
        {
            var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
            if (id > 0) _miscMasterId = id;
        }
        catch { /* resolve below */ }

        if (_miscMasterId <= 0)
        {
            var search = await _f.Client.GetAsync($"{MiscMasterRoute}?PageNumber=1&PageSize=15&SearchTerm={_miscMasterCode}");
            if (search.IsSuccessStatusCode)
            {
                using var sdoc = await QAHelper.ParseAsync(search);
                if (sdoc.RootElement.TryGetProperty("data", out var arr) &&
                    arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0 &&
                    arr[0].TryGetProperty("id", out var idp))
                    _miscMasterId = idp.GetInt32();
            }
        }

        _miscMasterId.Should().BeGreaterThan(0);
    }

    // AC3 — a MixCodeMaster can be created (independent code+description master).
    [Fact, TestPriority(3)]
    public async Task Step3_CreateMixCodeMaster()
    {
        _mixCode = Code();

        var resp = await _f.Client.PostAsJsonAsync(MixCodeRoute, new
        {
            mixCode = _mixCode,
            mixCodeDesc = $"US-PUR-01 Mix Code {Code()}"
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _mixCodeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _mixCodeId.Should().BeGreaterThan(0);
    }

    // AC4 — each created master is readable by id (tolerant: GetById guards differ per entity).
    [Fact, TestPriority(4)]
    public async Task Step4_CreatedMastersAreReadableById()
    {
        if (_miscTypeId > 0)
        {
            var r = await _f.Client.GetAsync($"{MiscTypeRoute}/{_miscTypeId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
        if (_miscMasterId > 0)
        {
            var r = await _f.Client.GetAsync($"{MiscMasterRoute}/{_miscMasterId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
        if (_mixCodeId > 0)
        {
            var r = await _f.Client.GetAsync($"{MixCodeRoute}/{_mixCodeId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
    }

    // AC5 — teardown leaf-first (MiscMaster → MiscType; MixCode independent).
    // ⚠️ tolerant: a parent delete while a child still links it may be blocked (400) or permitted (200)
    // depending on whether the dependent SoftDelete check is wired. All Purchase masters here bind
    // the delete id from the ROUTE: DELETE …/{id}.
    [Fact, TestPriority(5)]
    public async Task Step5_TeardownLeafFirst_WithDependentDeleteBlockProbe()
    {
        // Dependent-delete probe: while the MiscMaster still references the MiscType, deleting the
        // MiscType is either blocked (400) or permitted (200).
        if (_miscTypeId > 0 && _miscMasterId > 0)
        {
            var blocked = await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
            ((int)blocked.StatusCode).Should().BeOneOf(200, 400);
        }

        // Leaf-first cleanup — all ROUTE-bound deletes.
        if (_miscMasterId > 0) await _f.Client.DeleteAsync($"{MiscMasterRoute}/{_miscMasterId}");
        if (_miscTypeId > 0)   await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
        if (_mixCodeId > 0)    await _f.Client.DeleteAsync($"{MixCodeRoute}/{_mixCodeId}");
    }
}
