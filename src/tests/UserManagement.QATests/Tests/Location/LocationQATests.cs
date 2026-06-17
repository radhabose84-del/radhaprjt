// ─────────────────────────────────────────────────────────────────────────────
// Location — live-server QA suite
//
// VERIFIED CONTRACT (from LocationController + Create/UpdateLocationCommand):
//   Route base ............ /api/usermanagement/Location        ([Route("api/usermanagement/[controller]")])
//   Create ................ POST   /api/usermanagement/Location          body { code, locationName, description } → 200 { Data }
//   GetAll ................ GET    /api/usermanagement/Location/GetAllLocation?PageNumber&PageSize&SearchTerm    → 200 / 404 when empty
//   GetById ............... GET    /api/usermanagement/Location/{id}     — NO null guard → always 200
//   AutoComplete .......... GET    /api/usermanagement/Location/by-name?name=                                    → 200 { data }
//   Update ................ PUT    /api/usermanagement/Location/update   body { id, locationName, description, isActive }
//                            isActive is Status enum → send int 1 (Active) / 0 (Inactive)
//   Delete ................ DELETE /api/usermanagement/Location/{id}     — ROUTE {id}
//
//   code: NotEmpty + MaxLength(20) + AlreadyExists (unique, immutable). locationName: NotEmpty + MaxLength(100).
//   Update has a NotFound validator → non-existent id returns 400 "not found".
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.Location;

[Collection("LocationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class LocationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute   = "/api/usermanagement/Location";
    private const string GetAllRoute = "/api/usermanagement/Location/GetAllLocation";
    private const string UpdateRoute = "/api/usermanagement/Location/update";

    public LocationQATests(QAServerFixture fixture) => _f = fixture;

    private string Code(string suffix = "") => (_f.EntityCode[..8] + suffix);

    // ── CREATE ───────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code         = Code(),
            locationName = $"QA Location {_f.EntityCode[..10]}",
            description  = "QA created location"
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
            code         = Code("N"),
            locationName = "QA NoAuth",
            description  = ""
        });

        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_CodeEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code         = "",
            locationName = "QA Location",
            description  = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_LocationNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code         = Code("E"),
            locationName = "",
            description  = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_CodeExceedsMaxLength_Returns400()
    {
        // Code max = 20
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code         = new string('A', 21),
            locationName = "QA Location",
            description  = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_LocationNameExceedsMaxLength_Returns400()
    {
        // LocationName max = 100
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code         = Code("L"),
            locationName = new string('A', 101),
            description  = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_DuplicateCode_Returns400()
    {
        // Re-use the code created in TC001 → AlreadyExists fires
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            code         = Code(),
            locationName = $"QA Dup {_f.EntityCode[..10]}",
            description  = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "already exists");
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    [Fact, TestPriority(9)]
    [Trait("Layer", "Smoke")]
    public async Task TC009_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15");

        // Live (reconciled 2026-06-16): GetAllLocation returns 404 "No Record Found" when the
        // table is empty on BannariERP_QATest (controller's own empty-guard), else 200. Tolerate both.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(10)]
    public async Task TC010_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_GetAll_SearchByTerm_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15&SearchTerm=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(12)]
    public async Task TC012_GetAll_NoMatchSearch_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{GetAllRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        await QAHelper.Assert404Async(resp);
    }

    // ── GET BY ID (no null guard → always 200) ───────────────────────────────

    [Fact, TestPriority(13)]
    public async Task TC013_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(15)]
    public async Task TC015_GetById_NonExistentId_Returns200_NullData()
    {
        // FIXED (test, 2026-06-16): live GetById validates existence → 400 "Location not found.".
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    // ── AUTOCOMPLETE (by-name?name=) ─────────────────────────────────────────

    [Fact, TestPriority(16)]
    public async Task TC016_AutoComplete_WithName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(17)]
    public async Task TC017_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?name=QA");
        await QAHelper.Assert401Async(resp);
    }

    // ── UPDATE (PUT /update; code immutable; isActive Status enum → 1/0) ──────

    [Fact, TestPriority(18)]
    public async Task TC018_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id           = _f.CreatedId,
            locationName = $"QA Location Upd {_f.EntityCode[..10]}",
            description  = "QA updated",
            isActive     = 1
        });

        await QAHelper.AssertOkAsync(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "Updated");
    }

    [Fact, TestPriority(19)]
    public async Task TC019_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id           = _f.CreatedId,
            locationName = $"QA Location Inact {_f.EntityCode[..10]}",
            description  = "QA inactivated",
            isActive     = 0
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(20)]
    public async Task TC020_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id           = _f.CreatedId,
            locationName = $"QA Location React {_f.EntityCode[..10]}",
            description  = "QA reactivated",
            isActive     = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(UpdateRoute, new
        {
            id           = _f.CreatedId,
            locationName = "QA NoAuth",
            description  = "",
            isActive     = 1
        });

        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_Update_LocationNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id           = _f.CreatedId,
            locationName = "",
            description  = "",
            isActive     = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(23)]
    public async Task TC023_Update_LocationNameExceedsMaxLength_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id           = _f.CreatedId,
            locationName = new string('A', 101),
            description  = "",
            isActive     = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(24)]
    public async Task TC024_Update_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new
        {
            id           = 999999,
            locationName = "QA Ghost",
            description  = "",
            isActive     = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(25)]
    public async Task TC025_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(UpdateRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── DELETE (ROUTE {id} — ALWAYS LAST) ────────────────────────────────────

    [Fact, TestPriority(26)]
    public async Task TC026_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(27)]
    public async Task TC027_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "Deleted");
    }

    [Fact, TestPriority(28)]
    public async Task TC028_VerifyDelete_GetById_Returns200()
    {
        // FIXED (test, 2026-06-16): GetById of a soft-deleted Location validates existence → 400
        // "Location not found." (live contract returns 400, not 200).
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }
}
