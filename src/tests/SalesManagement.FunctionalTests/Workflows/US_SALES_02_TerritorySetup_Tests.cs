namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-02 — Sales territory setup
//   As a sales administrator I set up the field hierarchy
//   (SalesOrganisation → SalesOffice → SalesGroup) used to route sales activity.
// Fully implementable: all three are clean masters covered by the QA suite.
//
// Contracts (verified against SalesManagement.QATests, 2026-06-15):
//   POST /api/SalesOrganisation { salesOrganisationCode, salesOrganisationName, companyId, description? }
//   POST /api/SalesOffice       { salesOfficeName, salesOrganisationId, cityId, responsibleManager?, regionTerritory?, address? }
//   PUT  /api/SalesOffice       { id, salesOfficeName, salesOrganisationId, cityId, ..., isActive }
//   POST /api/SalesGroup        { salesGroupName, salesOfficeId, responsibleManager?, productCategoryId?, regionTerritory? }
//   PUT  /api/SalesGroup        { id, salesGroupName, salesOfficeId, ..., isActive }
//   DELETE /api/{Entity}?id={id}  (id bound from QUERY)
//   GET  /api/SalesGroup/by-name?term=
//
// Note: SalesOffice name has an Alphanumeric rule — use plain alphanumeric tokens (no specials).
//       cityId is a cross-module FK; resolved via _f.CityId (fallback 1).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-02-TerritorySetup")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_02_TerritorySetup_Tests
{
    private readonly QAServerFixture _f;

    private const string OrgRoute    = "/api/SalesOrganisation";
    private const string OfficeRoute = "/api/SalesOffice";
    private const string GroupRoute  = "/api/SalesGroup";

    private const int ValidCompanyId = 1;

    private static int _orgId;
    private static int _officeId;
    private static int _groupId;

    // Run-unique names captured so reads / autocomplete steps can assert on them.
    private static string _officeName = string.Empty;
    private static string _groupName  = string.Empty;

    public US_SALES_02_TerritorySetup_Tests(QAServerFixture fixture) => _f = fixture;

    private string Code() => _f.EntityCode[..10];

    private int CityId() => _f.CityId > 0 ? _f.CityId : 1;

    // AC1 — a SalesOrganisation exists (created in-flow).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateSalesOrganisation()
    {
        var resp = await _f.Client.PostAsJsonAsync(OrgRoute, new
        {
            salesOrganisationCode = Code(),
            salesOrganisationName = "QA Territory Organisation",
            companyId = ValidCompanyId,
            description = "US-SALES-02"
        });
        await QAHelper.AssertOkAsync(resp);
        _orgId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _orgId.Should().BeGreaterThan(0);
    }

    // AC2 — a SalesOffice can be created under the organisation (city FK resolved at runtime).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateSalesOfficeUnderOrganisation()
    {
        _orgId.Should().BeGreaterThan(0, "Step1 must have created the organisation");

        // Alphanumeric name (no spaces/specials) to satisfy the SalesOffice Alphanumeric rule.
        _officeName = $"QASalesOffice{_f.EntityCode[1..7]}";

        var resp = await _f.Client.PostAsJsonAsync(OfficeRoute, new
        {
            salesOfficeName = _officeName,
            salesOrganisationId = _orgId,
            cityId = CityId(),
            responsibleManager = "QA Manager",
            regionTerritory = "QA Region",
            address = "QA Address"
        });
        await QAHelper.AssertOkAsync(resp);
        _officeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _officeId.Should().BeGreaterThan(0);
    }

    // AC3 — a SalesGroup can be created under the office.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateSalesGroupUnderOffice()
    {
        _officeId.Should().BeGreaterThan(0, "Step2 must have created the office");

        _groupName = $"QA Sales Group {_f.EntityCode[..6]}";

        var resp = await _f.Client.PostAsJsonAsync(GroupRoute, new
        {
            salesGroupName = _groupName,
            salesOfficeId = _officeId,
            responsibleManager = "QA Manager",
            regionTerritory = "QA Region"
        });
        await QAHelper.AssertOkAsync(resp);
        _groupId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _groupId.Should().BeGreaterThan(0);
    }

    // AC4 — both are readable by id and appear in autocomplete.
    [Fact, TestPriority(4)]
    public async Task Step4_OfficeAndGroupReadableById_AndInAutocomplete()
    {
        _officeId.Should().BeGreaterThan(0);
        _groupId.Should().BeGreaterThan(0);

        var officeById = await _f.Client.GetAsync($"{OfficeRoute}/{_officeId}");
        await QAHelper.AssertOkAsync(officeById);
        (await QAHelper.ParseAsync(officeById)).RootElement
            .GetProperty("data").GetProperty("salesOfficeName").GetString()
            .Should().Be(_officeName);

        var groupById = await _f.Client.GetAsync($"{GroupRoute}/{_groupId}");
        await QAHelper.AssertOkAsync(groupById);
        (await QAHelper.ParseAsync(groupById)).RootElement
            .GetProperty("data").GetProperty("salesGroupName").GetString()
            .Should().Be(_groupName);

        // Both are active → must appear in their by-name autocomplete.
        var officeAuto = await _f.Client.GetAsync($"{OfficeRoute}/by-name?term={_f.EntityCode[1..7]}");
        await QAHelper.AssertOkAsync(officeAuto);
        var officeNames = (await QAHelper.ParseAsync(officeAuto)).RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(e => e.TryGetProperty("salesOfficeName", out var n) ? n.GetString() : null)
            .ToList();
        officeNames.Should().Contain(_officeName, "the active office must appear in autocomplete");

        var groupAuto = await _f.Client.GetAsync($"{GroupRoute}/by-name?term={_f.EntityCode[..6]}");
        await QAHelper.AssertOkAsync(groupAuto);
        var groupNames = (await QAHelper.ParseAsync(groupAuto)).RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(e => e.TryGetProperty("salesGroupName", out var n) ? n.GetString() : null)
            .ToList();
        groupNames.Should().Contain(_groupName, "the active group must appear in autocomplete");
    }

    // AC5 — deactivating the SalesGroup excludes it from autocomplete, keeps it in GetAll.
    // ⚠️ verify against live: autocomplete filters IsActive=1 AND IsDeleted=0; GetAll filters IsDeleted=0 only.
    [Fact, TestPriority(5)]
    public async Task Step5_DeactivateGroup_ExcludedFromAutocomplete_PresentInGetAll()
    {
        _groupId.Should().BeGreaterThan(0, "Step3 must have created the group");

        var deactivate = await _f.Client.PutAsJsonAsync(GroupRoute, new
        {
            id = _groupId,
            salesGroupName = _groupName,
            salesOfficeId = _officeId,
            responsibleManager = "QA Manager",
            regionTerritory = "QA Region",
            isActive = 0
        });
        await QAHelper.AssertOkAsync(deactivate);

        // Excluded from active autocomplete.
        var auto = await _f.Client.GetAsync($"{GroupRoute}/by-name?term={_f.EntityCode[..6]}");
        await QAHelper.AssertOkAsync(auto);
        var autoNames = (await QAHelper.ParseAsync(auto)).RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(e => e.TryGetProperty("salesGroupName", out var n) ? n.GetString() : null)
            .ToList();
        autoNames.Should().NotContain(_groupName,
            "a deactivated group must be hidden from active autocomplete");

        // Still present in GetAll.
        var all = await _f.Client.GetAsync($"{GroupRoute}?PageNumber=1&PageSize=50&SearchTerm={_f.EntityCode[..6]}");
        await QAHelper.AssertOkAsync(all);
        (await QAHelper.ParseAsync(all)).RootElement.GetProperty("data").GetArrayLength()
            .Should().BeGreaterThan(0, "a deactivated (not deleted) group must remain in GetAll");
    }

    // AC6 — teardown leaf-first (group → office → organisation).
    [Fact, TestPriority(6)]
    public async Task Step6_TeardownLeafFirst()
    {
        if (_groupId > 0)  await _f.Client.DeleteAsync($"{GroupRoute}?id={_groupId}");
        if (_officeId > 0) await _f.Client.DeleteAsync($"{OfficeRoute}?id={_officeId}");
        if (_orgId > 0)    await _f.Client.DeleteAsync($"{OrgRoute}?id={_orgId}");
    }
}
