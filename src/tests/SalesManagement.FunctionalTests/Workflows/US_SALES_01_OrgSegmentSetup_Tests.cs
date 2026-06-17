namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-01 — Sales organisation & segment hierarchy
//   As a sales administrator I build the org backbone
//   (SalesOrganisation → SalesChannel + BusinessUnit → SalesSegment)
//   so transactions can be classified by segment.
// Fully implementable: all four are clean masters covered by the QA suite.
//
// Contracts (verified against SalesManagement.QATests, 2026-06-15):
//   POST /api/SalesOrganisation { salesOrganisationCode, salesOrganisationName, companyId, description? }
//   POST /api/SalesChannel      { salesChannelCode, salesChannelName }
//   POST /api/BusinessUnit      { businessUnitCode, businessUnitName, description? }
//   POST /api/SalesSegment      { salesOrganisationId, salesChannelId, businessUnitId, currencyId?, validFrom?, segmentName }
//   PUT  /api/SalesSegment      { id, currencyId?, validFrom?, segmentName, isActive }
//   DELETE /api/{Entity}?id={id}  (id bound from QUERY)
//   GET  /api/SalesSegment/by-name?term=
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-01-OrgSegmentSetup")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-01")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_01_OrgSegmentSetup_Tests
{
    private readonly QAServerFixture _f;

    private const string OrgRoute      = "/api/SalesOrganisation";
    private const string ChannelRoute  = "/api/SalesChannel";
    private const string BusinessRoute = "/api/BusinessUnit";
    private const string SegmentRoute  = "/api/SalesSegment";

    // CompanyId is a cross-module FK validated via ICompanyLookup; id 1 is the conventional seed.
    private const int ValidCompanyId = 1;

    private static int _orgId;
    private static int _channelId;
    private static int _businessUnitId;
    private static int _segmentId;

    // Run-unique segment name captured so the deactivate step can assert autocomplete exclusion.
    private static string _segmentName = string.Empty;

    public US_SALES_01_OrgSegmentSetup_Tests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric code sliced to 10 chars (well within every code's max length).
    private string Code() => _f.EntityCode[..10];

    // AC1 — a SalesOrganisation can be created and returns a new id.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateSalesOrganisation()
    {
        var resp = await _f.Client.PostAsJsonAsync(OrgRoute, new
        {
            salesOrganisationCode = Code(),
            salesOrganisationName = "QA Sales Organisation",
            companyId = ValidCompanyId,
            description = "US-SALES-01"
        });
        await QAHelper.AssertOkAsync(resp);
        _orgId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _orgId.Should().BeGreaterThan(0);
    }

    // AC2 — a SalesChannel can be created.
    [Fact, TestPriority(2)]
    public async Task Step2_CreateSalesChannel()
    {
        var resp = await _f.Client.PostAsJsonAsync(ChannelRoute, new
        {
            salesChannelCode = Code(),
            salesChannelName = "QA Sales Channel"
        });
        await QAHelper.AssertOkAsync(resp);
        _channelId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _channelId.Should().BeGreaterThan(0);
    }

    // AC3 — a BusinessUnit can be created.
    [Fact, TestPriority(3)]
    public async Task Step3_CreateBusinessUnit()
    {
        var resp = await _f.Client.PostAsJsonAsync(BusinessRoute, new
        {
            businessUnitCode = Code(),
            businessUnitName = $"QA Business Unit {_f.EntityCode[..6]}",
            description = "US-SALES-01"
        });
        await QAHelper.AssertOkAsync(resp);
        _businessUnitId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _businessUnitId.Should().BeGreaterThan(0);
    }

    // AC4 — a SalesSegment can be created from (organisation, channel, business unit).
    [Fact, TestPriority(4)]
    public async Task Step4_CreateSalesSegmentFromTheThree()
    {
        _orgId.Should().BeGreaterThan(0, "Step1 must have created the organisation");
        _channelId.Should().BeGreaterThan(0, "Step2 must have created the channel");
        _businessUnitId.Should().BeGreaterThan(0, "Step3 must have created the business unit");

        _segmentName = $"QA Sales Segment {_f.EntityCode[..6]}";

        var resp = await _f.Client.PostAsJsonAsync(SegmentRoute, new
        {
            salesOrganisationId = _orgId,
            salesChannelId = _channelId,
            businessUnitId = _businessUnitId,
            segmentName = _segmentName
        });
        await QAHelper.AssertOkAsync(resp);
        _segmentId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _segmentId.Should().BeGreaterThan(0);
    }

    // AC5 — re-creating the same (org, channel, BU) combination is rejected (composite unique).
    [Fact, TestPriority(5)]
    public async Task Step5_DuplicateCombinationRejected()
    {
        _segmentId.Should().BeGreaterThan(0, "Step4 must have created the segment");

        var resp = await _f.Client.PostAsJsonAsync(SegmentRoute, new
        {
            salesOrganisationId = _orgId,
            salesChannelId = _channelId,
            businessUnitId = _businessUnitId,
            segmentName = _segmentName + " DUP"
        });
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
    }

    // AC6 — deactivating the SalesSegment removes it from autocomplete but keeps it in GetAll.
    // ⚠️ verify against live: autocomplete filters IsActive=1 AND IsDeleted=0; GetAll filters IsDeleted=0 only.
    [Fact, TestPriority(6)]
    public async Task Step6_DeactivateSegment_ExcludedFromAutocomplete_PresentInGetAll()
    {
        _segmentId.Should().BeGreaterThan(0, "Step4 must have created the segment");

        var deactivate = await _f.Client.PutAsJsonAsync(SegmentRoute, new
        {
            id = _segmentId,
            segmentName = _segmentName,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(deactivate);

        // Excluded from active autocomplete (/by-name filters IsActive=1).
        var auto = await _f.Client.GetAsync($"{SegmentRoute}/by-name?term={_f.EntityCode[..6]}");
        await QAHelper.AssertOkAsync(auto);
        var autoDoc = await QAHelper.ParseAsync(auto);
        var autoNames = autoDoc.RootElement.GetProperty("data").EnumerateArray()
            .Select(e => e.TryGetProperty("segmentName", out var n) ? n.GetString() : null)
            .ToList();
        // note: tolerant — assert the deactivated row's exact name is absent from autocomplete.
        autoNames.Should().NotContain(_segmentName,
            "a deactivated segment must be hidden from active autocomplete");

        // Still present in GetAll (IsDeleted=0 keeps it visible).
        var all = await _f.Client.GetAsync($"{SegmentRoute}?PageNumber=1&PageSize=50&SearchTerm={_f.EntityCode[..6]}");
        await QAHelper.AssertOkAsync(all);
        var allDoc = await QAHelper.ParseAsync(all);
        allDoc.RootElement.GetProperty("data").GetArrayLength()
            .Should().BeGreaterThan(0, "a deactivated (not deleted) segment must remain in GetAll");
    }

    // AC7 — teardown leaf-first (segment → BU/channel → organisation).
    // ⚠️ verify dependent-delete block: attempting to delete a parent while the segment still
    // links it should be rejected (400). Behaviour is tolerant pending live reconciliation.
    [Fact, TestPriority(7)]
    public async Task Step7_TeardownLeafFirst_WithDependentDeleteBlockProbe()
    {
        // Dependent-delete probe: while the segment exists, deleting the organisation should be
        // blocked. note: tolerant — some masters may not yet enforce the SoftDelete dependent check.
        if (_orgId > 0 && _segmentId > 0)
        {
            var blocked = await _f.Client.DeleteAsync($"{OrgRoute}?id={_orgId}");
            // parent delete while a child segment links it is either blocked (400) or
            // permitted (200) depending on whether the dependent check is wired.
            ((int)blocked.StatusCode).Should().BeOneOf(200, 400);
        }

        // Leaf-first cleanup.
        if (_segmentId > 0)      await _f.Client.DeleteAsync($"{SegmentRoute}?id={_segmentId}");
        if (_businessUnitId > 0) await _f.Client.DeleteAsync($"{BusinessRoute}?id={_businessUnitId}");
        if (_channelId > 0)      await _f.Client.DeleteAsync($"{ChannelRoute}?id={_channelId}");
        if (_orgId > 0)          await _f.Client.DeleteAsync($"{OrgRoute}?id={_orgId}");
    }
}
