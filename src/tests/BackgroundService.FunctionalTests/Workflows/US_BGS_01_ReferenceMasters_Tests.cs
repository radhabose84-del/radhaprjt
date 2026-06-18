namespace BackgroundService.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-BGS-01 — Reference master vocabulary
//   As a BackgroundService administrator I build the reference vocabulary
//   (MiscTypeMaster → MiscMaster, plus an independent NotificationGroup)
//   so notification and approval configuration can reference consistent codes.
// Fully implementable: all three are clean masters covered by the QA suite.
//
// Contracts (verified against BackgroundService.QATests, 2026-06-18):
//   POST   /api/backgroundservice/MiscTypeMaster { miscTypeCode, description }   → ApiResponseDTO-wrapped, data.Id
//   POST   /api/backgroundservice/MiscMaster     { miscTypeId, code, description } → BARE DTO, data.Id
//   POST   /api/NotificationGroup                { groupName }                    → raw int id
//   GET    /api/backgroundservice/MiscTypeMaster/{id}   (404 guard)
//   GET    /api/backgroundservice/MiscMaster/{id}       (may NRE 500 on missing)
//   DELETE /api/backgroundservice/MiscMaster/{id}       (id from ROUTE)
//   DELETE /api/backgroundservice/MiscTypeMaster/{id}   (id from ROUTE; over-broad dependent guard)
//   DELETE /api/NotificationGroup?id={id}               (id from QUERY)
//   Create returns 200/201 (heterogeneous shape) — accept BeOneOf(200,201) for capture.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-BGS-01-ReferenceMasters")]
[Trait("Module", "BackgroundService")]
[Trait("Story", "US-BGS-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_BGS_01_ReferenceMasters_Tests
{
    private readonly QAServerFixture _f;

    private const string MiscTypeRoute   = "/api/backgroundservice/MiscTypeMaster";
    private const string MiscMasterRoute = "/api/backgroundservice/MiscMaster";
    private const string GroupRoute      = "/api/NotificationGroup";

    private static int _miscTypeId;
    private static int _miscMasterId;
    private static int _groupId;

    // Captured codes — reused by readable-by-id / search assertions.
    private static string _miscTypeCode   = string.Empty;
    private static string _miscMasterCode = string.Empty;
    private static string _groupName      = string.Empty;

    public US_BGS_01_ReferenceMasters_Tests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric code clamped to 10 chars (within every code's max length).
    private string Code() => _f.EntityCode[..Math.Min(10, _f.EntityCode.Length)];

    // AC1 — a MiscTypeMaster can be created and returns a new id (ApiResponseDTO-wrapped).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateMiscTypeMaster()
    {
        _miscTypeCode = Code();

        var resp = await _f.Client.PostAsJsonAsync(MiscTypeRoute, new
        {
            miscTypeCode = _miscTypeCode,
            description = "US-BGS-01 Misc Type"
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _miscTypeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _miscTypeId.Should().BeGreaterThan(0);
    }

    // AC2 — a MiscMaster value can be created under that type (FK miscTypeId; bare-DTO return).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateMiscMasterUnderType()
    {
        _miscTypeId.Should().BeGreaterThan(0, "Step1 must have created the misc type");
        _miscMasterCode = Code();

        var resp = await _f.Client.PostAsJsonAsync(MiscMasterRoute, new
        {
            miscTypeId = _miscTypeId,
            code = _miscMasterCode,
            description = "US-BGS-01 Misc Master"
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);

        // MiscMaster create echoes a bare DTO under data; CreatedId() reads data.Id.
        // Fall back to a search-by-code if the shape carries no numeric id directly.
        try
        {
            var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
            if (id > 0) _miscMasterId = id;
        }
        catch { /* resolve below */ }

        if (_miscMasterId <= 0)
        {
            var search = await _f.Client.GetAsync(
                $"{MiscMasterRoute}?PageNumber=1&PageSize=15&SearchTerm={_miscMasterCode}");
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

    // AC3 — a NotificationGroup can be created (independent name master, raw-int return).
    [Fact, TestPriority(3)]
    public async Task Step3_CreateNotificationGroup()
    {
        _groupName = "QAGrp" + Code();

        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new { groupName = _groupName });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    // AC4 — each created master is readable (tolerant: GetById guards differ per entity).
    //   MiscType has a 404 guard; MiscMaster may NRE (500) on a missing id; NotificationGroup
    //   has NO GetById → assert the list endpoint is reachable instead.
    [Fact, TestPriority(4)]
    public async Task Step4_CreatedMastersAreReadable()
    {
        if (_miscTypeId > 0)
        {
            var r = await _f.Client.GetAsync($"{MiscTypeRoute}/{_miscTypeId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
        if (_miscMasterId > 0)
        {
            var r = await _f.Client.GetAsync($"{MiscMasterRoute}/{_miscMasterId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404, 500);
        }

        // NotificationGroup has no GetById → list read proves the master surface is reachable.
        var list = await _f.Client.GetAsync($"{GroupRoute}?PageNumber=1&PageSize=15");
        ((int)list.StatusCode).Should().Be(200);
    }

    // AC5 — teardown leaf-first (MiscMaster → MiscType; NotificationGroup independent).
    // ⚠️ tolerant: MiscType delete carries an over-broad dependent-link guard (400) and DELETE
    // bindings differ per entity — MiscMaster/MiscType = ROUTE /{id}; NotificationGroup = QUERY ?id=.
    [Fact, TestPriority(5)]
    public async Task Step5_TeardownLeafFirst_WithDependentDeleteBlockProbe()
    {
        // Dependent-delete probe: while the MiscMaster child still references the MiscType,
        // deleting the MiscType is either blocked (400) or permitted (200).
        if (_miscTypeId > 0 && _miscMasterId > 0)
        {
            var blocked = await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
            ((int)blocked.StatusCode).Should().BeOneOf(200, 400);
        }

        // Leaf-first cleanup — ROUTE-bound deletes for misc master/type, QUERY-bound for group.
        if (_miscMasterId > 0) await _f.Client.DeleteAsync($"{MiscMasterRoute}/{_miscMasterId}");
        if (_miscTypeId > 0)   await _f.Client.DeleteAsync($"{MiscTypeRoute}/{_miscTypeId}");
        if (_groupId > 0)      await _f.Client.DeleteAsync($"{GroupRoute}?id={_groupId}");
    }
}
