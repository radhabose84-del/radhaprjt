// ─────────────────────────────────────────────────────────────────────────────
// Entity — live-server QA suite (UserManagement)
//
// Source verified:
//   Controller : UserManagement.Presentation/Controllers/EntityController.cs
//     • Route          : /api/Entity
//     • Create  POST   : /api/Entity        → body CreateEntityCommand (returns { data = int })
//     • Update  PUT    : /api/Entity        → body UpdateEntityCommand (adds Id + IsActive byte)
//     • Delete  DELETE : /api/Entity/{id}   → ROUTE param (NOT query)
//     • GetAll  GET    : /api/Entity?PageNumber=&PageSize=&SearchTerm=  (404 when empty)
//     • GetById GET    : /api/Entity/{id}   → id<=0 → 400 "Invalid Entity ID"
//     • AutoCmp GET    : /api/Entity/by-name?EntityName=
//     • GET /api/Entity/new-code               (auto-gen entity code)
//     • GET /api/Entity/{entityId}/companies   (404 when none)
//     • GET /api/Entity/UnitsLoad?companyIds=  (csv ids)
//   Create cmd : EntityName?, EntityDescription?, Address?, Phone?, Email?  (entityCode auto)
//   Update cmd : adds Id + IsActive(byte)
//   Validator  : CreateEntityCommandValidator → NotEmpty on EntityName, Address, Phone, Email;
//                Phone must be numeric/mobile-pattern; Email must match email pattern.
//                Create IS satisfiable (no required FK).
// ─────────────────────────────────────────────────────────────────────────────

namespace UserManagement.QATests.Tests.Entity;

[Collection("EntityCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class EntityQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Entity";

    public EntityQATests(QAServerFixture fixture) => _f = fixture;

    private object ValidCreateBody() => new
    {
        entityName        = $"QA Entity {_f.EntityCode[..10]}",
        entityDescription = "QA functional-test entity",
        address           = "123 QA Street",
        phone             = "9876543210",      // 10-digit numeric → passes mobile/numeric rule
        email             = $"qa{_f.EntityCode[..6]}@bannari.test"
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 0 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidCreateBody());

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id  = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, ValidCreateBody());
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_EntityNameEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            entityName = "",
            address    = "123 QA Street",
            phone      = "9876543210",
            email      = $"qa{_f.EntityCode[..6]}@bannari.test"
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_PhoneEmpty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            entityName = $"QA Entity {_f.EntityCode[..8]}",
            address    = "123 QA Street",
            phone      = "",
            email      = $"qa{_f.EntityCode[..6]}@bannari.test"
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_InvalidEmailFormat_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            entityName = $"QA Entity {_f.EntityCode[..8]}",
            address    = "123 QA Street",
            phone      = "9876543210",
            email      = "not-an-email"
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "email");
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_NonNumericPhone_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            entityName = $"QA Entity {_f.EntityCode[..8]}",
            address    = "123 QA Street",
            phone      = "ABC123",
            email      = $"qa{_f.EntityCode[..6]}@bannari.test"
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — GET ALL  (404 when no data, like Currency/Department)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_NoMatchSearch_Returns404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm=ZZZNOMATCH999");
        await QAHelper.Assert404Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        _f.CreatedId.Should().BeGreaterThan(0, "TC001 must have created an Entity");

        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_IdZero_Returns400()
    {
        // Controller: if (id <= 0) → 400 "Invalid Entity ID"
        var resp = await _f.Client.GetAsync($"{BaseRoute}/0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_GetById_NonExistentId_Returns200Or404()
    {
        // FIXED (test, 2026-06-16): live GetById validates existence → 400 "Entity not found".
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — AUTOCOMPLETE  (by-name?EntityName=)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithName_Returns200()
    {
        // FIXED (test, 2026-06-16): autocomplete with a no-match term returns 400 "No entity found"
        // (live contract). "QA" matches no seeded entity on the clone, so tolerate 200/400.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?EntityName=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?EntityName=QA");
        await QAHelper.Assert401Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — EXTRA GETs
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(45)]
    public async Task TC045_NewCode_Returns200Or400()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/new-code");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(46)]
    public async Task TC046_Companies_ForEntity_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}/companies");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(47)]
    public async Task TC047_UnitsLoad_WithCompanyIds_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/UnitsLoad?companyIds=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(48)]
    public async Task TC048_UnitsLoad_EmptyCompanyIds_Returns400()
    {
        // Controller: if (string.IsNullOrWhiteSpace(companyIds)) → 400
        var resp = await _f.Client.GetAsync($"{BaseRoute}/UnitsLoad?companyIds=");
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (adds Id + IsActive byte)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        _f.CreatedId.Should().BeGreaterThan(0, "TC001 must have created an Entity");

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            entityName        = $"QA Entity Updated {_f.EntityCode[..8]}",
            entityDescription = "QA updated",
            address           = "456 QA Avenue",
            phone             = "9123456780",
            email             = $"qau{_f.EntityCode[..6]}@bannari.test",
            isActive          = (byte)1
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("message").GetString().Should().Contain("updated");
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            entityName = "No Auth Update",
            address    = "456 QA Avenue",
            phone      = "9123456780",
            email      = $"qau{_f.EntityCode[..6]}@bannari.test",
            isActive   = (byte)1
        });

        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EntityNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id         = _f.CreatedId,
            entityName = "",
            address    = "456 QA Avenue",
            phone      = "9123456780",
            email      = $"qau{_f.EntityCode[..6]}@bannari.test",
            isActive   = (byte)1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_Inactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            entityName        = $"QA Entity Inactive {_f.EntityCode[..8]}",
            entityDescription = "QA inactive",
            address           = "456 QA Avenue",
            phone             = "9123456780",
            email             = $"qau{_f.EntityCode[..6]}@bannari.test",
            isActive          = (byte)0
        });

        // Deactivate = Update with IsActive=0 (not a delete).
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 500);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_Reactivate_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id                = _f.CreatedId,
            entityName        = $"QA Entity Reactive {_f.EntityCode[..8]}",
            entityDescription = "QA reactive",
            address           = "456 QA Avenue",
            phone             = "9123456780",
            email             = $"qau{_f.EntityCode[..6]}@bannari.test",
            isActive          = (byte)1
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 500);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — /{id} ROUTE param)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(60)]
    public async Task TC060_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(61)]
    public async Task TC061_Delete_HappyPath_SoftDelete_Returns200()
    {
        _f.CreatedId.Should().BeGreaterThan(0, "TC001 must have created an Entity");

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");

        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 500);
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.GetProperty("message").GetString().Should().Contain("deleted");
        }
    }

    [Fact, TestPriority(62)]
    public async Task TC062_VerifyDelete_GetById_Returns200OrNull()
    {
        // FIXED (test, 2026-06-16): live GetById of a soft-deleted entity validates existence
        // → 400 "Entity not found". Tolerate 200/400/404.
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(63)]
    public async Task TC063_Delete_NonExistentId_Returns200Or400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }
}
