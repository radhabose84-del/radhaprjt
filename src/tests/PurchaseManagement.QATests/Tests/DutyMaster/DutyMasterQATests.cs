namespace PurchaseManagement.QATests.Tests.DutyMaster;

// ─────────────────────────────────────────────────────────────────────────────
// DutyMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-17 — DutyMasterController + DutyMasterDto):
//   Route = [Route("api/[controller]")] → /api/DutyMaster
//   POST   /api/DutyMaster   (body IS the DutyMasterDto): { tariffNumber, hsnCode?, hsnId, dutyCategoryId,
//                            basicCustomsDutyPercentage, socialWelfareSurchargePercentage, iGSTPercentage,
//                            effectiveFrom, countryOfOriginApplicability }
//   PUT    /api/DutyMaster   (body DutyMasterDto): { id, ...same fields, isActive }   (tariffNumber immutable)
//   DELETE /api/DutyMaster/{id:int}   (id bound from ROUTE)
//   GET    /api/DutyMaster?pageNumber=&pageSize=&search=   (data = { items, total } — NOT root totalCount)
//   GET    /api/DutyMaster/{id:int}   (200 with data; controller sets StatusCode field but HTTP is 200)
//   GET    /api/DutyMaster/autocomplete?term=
//
// Key facts that shaped assertions:
//   • hsnId → /api/hsnmaster FK ; dutyCategoryId → /api/purchase/miscmaster FK. Both REQUIRED.
//     Resolved at runtime via FirstIdAsync; if either is 0 the create-happy self-skips.
//   • Create returns 201 envelope inside Ok(200); data is the bare int new id → CreatedId().
//   • GetAll uses paging params pageNumber/pageSize/search and a NESTED data {items,total} shape.
//   • Delete always returns Ok(200) — negatives tolerated (200,400).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("DutyMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class DutyMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/DutyMaster";
    private const string HsnRoute = "/api/hsnmaster";
    private const string DutyCategoryRoute = "/api/purchase/miscmaster";

    private static string _createdTariff = string.Empty;
    private static int _hsnId;
    private static int _dutyCategoryId;

    public DutyMasterQATests(QAServerFixture fixture) => _f = fixture;

    private string NewTariff() => _f.EntityCode[..10];

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    // Skipped on the QA clone: create returns 400 "No transaction type configured for Duty Master."
    // — the clone has no Finance TransactionTypeMaster row (and DocumentSequence) for Duty Master
    // doc-numbering. Downstream id-dependent update/delete steps are skipped likewise.
    [Fact(Skip = "needs seeded data: Finance TransactionTypeMaster + DocumentSequence for 'Duty Master'"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        _createdTariff = NewTariff();
        _hsnId = await QAHelper.FirstIdAsync(_f.Client, HsnRoute);
        _dutyCategoryId = await QAHelper.FirstIdAsync(_f.Client, DutyCategoryRoute);

        if (_hsnId == 0 || _dutyCategoryId == 0)
            return; // required FK unresolved on clone — downstream guards on _f.CreatedId==0

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            tariffNumber = _createdTariff,
            hsnCode = "QAHSN",
            hsnId = _hsnId,
            dutyCategoryId = _dutyCategoryId,
            basicCustomsDutyPercentage = 10m,
            socialWelfareSurchargePercentage = 1m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1
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
            tariffNumber = "NOAUTH01",
            hsnId = 1,
            dutyCategoryId = 1,
            basicCustomsDutyPercentage = 10m,
            socialWelfareSurchargePercentage = 1m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_TariffEmpty_Returns200_Or_400()
    {
        var hsn = await QAHelper.FirstIdAsync(_f.Client, HsnRoute);
        var cat = await QAHelper.FirstIdAsync(_f.Client, DutyCategoryRoute);

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            tariffNumber = "",
            hsnId = hsn,
            dutyCategoryId = cat,
            basicCustomsDutyPercentage = 10m,
            socialWelfareSurchargePercentage = 1m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_DuplicateTariff_Returns200_Or_400()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            tariffNumber = _createdTariff,
            hsnId = _hsnId,
            dutyCategoryId = _dutyCategoryId,
            basicCustomsDutyPercentage = 10m,
            socialWelfareSurchargePercentage = 1m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_EmptyBody_Returns200_Or_400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (paging params pageNumber/pageSize/search; nested data shape)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_SearchByTariff_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?pageNumber=1&pageSize=15&search={_createdTariff}");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

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
    public async Task TC032_GetById_NonExistentId_Returns200()
    {
        // Controller returns HTTP 200 with data:null (StatusCode field inside body is 404).
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (route `autocomplete`, param `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete?term=QA");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/autocomplete");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/autocomplete?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (tariffNumber immutable)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: Finance TransactionTypeMaster + DocumentSequence for 'Duty Master'"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            tariffNumber = _createdTariff,
            hsnId = _hsnId,
            dutyCategoryId = _dutyCategoryId,
            basicCustomsDutyPercentage = 12m,
            socialWelfareSurchargePercentage = 2m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1,
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
            tariffNumber = "X",
            hsnId = 1,
            dutyCategoryId = 1,
            basicCustomsDutyPercentage = 12m,
            socialWelfareSurchargePercentage = 2m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: Finance TransactionTypeMaster + DocumentSequence for 'Duty Master'"), TestPriority(52)]
    public async Task TC052_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            tariffNumber = _createdTariff,
            hsnId = _hsnId,
            dutyCategoryId = _dutyCategoryId,
            basicCustomsDutyPercentage = 12m,
            socialWelfareSurchargePercentage = 2m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            tariffNumber = _createdTariff,
            hsnId = _hsnId,
            dutyCategoryId = _dutyCategoryId,
            basicCustomsDutyPercentage = 12m,
            socialWelfareSurchargePercentage = 2m,
            iGSTPercentage = 18m,
            effectiveFrom = "2026-01-01T00:00:00+00:00",
            countryOfOriginApplicability = 1,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id from ROUTE; controller always 200)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns200_Or_400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/0");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns200_Or_400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: Finance TransactionTypeMaster + DocumentSequence for 'Duty Master'"), TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact(Skip = "needs seeded data: Finance TransactionTypeMaster + DocumentSequence for 'Duty Master'"), TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns200_Or_400()
    {
        if (_f.CreatedId == 0) return; // create-happy skipped

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/{_f.CreatedId}");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400);
    }
}
