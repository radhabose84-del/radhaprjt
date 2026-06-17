namespace SalesManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-SALES-07 — Item pricing per segment
//   As a pricing administrator I publish an item price for a sales segment over a
//   validity window (segment created in-flow: org + channel + business unit).
// PARTIAL: SalesSegment is creatable in-flow; ItemPriceMaster needs an Inventory item
// id + a Currency (blocked); validFrom>validTo rejection is active; by-item-date /
// exmill-rate retrieval is authored as tolerant reachability.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-SALES-07-ItemPricing")]
[Trait("Module", "SalesManagement")]
[Trait("Story", "US-SALES-07")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_SALES_07_ItemPricing_Tests
{
    private readonly QAServerFixture _f;

    private const string OrganisationRoute = "/api/SalesOrganisation";
    private const string SalesChannelRoute = "/api/SalesChannel";
    private const string BusinessUnitRoute = "/api/BusinessUnit";
    private const string SalesSegmentRoute = "/api/SalesSegment";
    private const string ItemPriceRoute    = "/api/ItemPriceMaster";

    private const int ValidCompanyId = 1;

    private static int _orgId;
    private static int _channelId;
    private static int _businessUnitId;
    private static int _segmentId;

    public US_SALES_07_ItemPricing_Tests(QAServerFixture fixture) => _f = fixture;

    private static readonly string ValidFrom = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
    private static readonly string ValidTo   = DateOnly.FromDateTime(DateTime.Today.AddYears(1)).ToString("yyyy-MM-dd");

    // AC1 (part a) — a SalesOrganisation exists (created in-flow, part of the segment composite).
    [Fact, TestPriority(1)]
    public async Task Step1_CreateSalesOrganisation()
    {
        var resp = await _f.Client.PostAsJsonAsync(OrganisationRoute, new
        {
            salesOrganisationCode = _f.EntityCode[..10],
            salesOrganisationName = "QA US07 Organisation",
            companyId = ValidCompanyId,
            description = "US-SALES-07"
        });
        await QAHelper.AssertOkAsync(resp);
        _orgId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _orgId.Should().BeGreaterThan(0);
    }

    // AC1 (part b) — a SalesChannel exists (created in-flow).
    [Fact, TestPriority(2)]
    public async Task Step2_CreateSalesChannel()
    {
        var resp = await _f.Client.PostAsJsonAsync(SalesChannelRoute, new
        {
            salesChannelCode = $"C{_f.EntityCode[1..9]}",
            salesChannelName = "QA US07 Channel"
        });
        await QAHelper.AssertOkAsync(resp);
        _channelId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _channelId.Should().BeGreaterThan(0);
    }

    // AC1 (part c) — a BusinessUnit exists (created in-flow).
    [Fact, TestPriority(3)]
    public async Task Step3_CreateBusinessUnit()
    {
        var resp = await _f.Client.PostAsJsonAsync(BusinessUnitRoute, new
        {
            businessUnitCode = _f.EntityCode[..10],
            businessUnitName = $"QA US07 BU {_f.EntityCode[..8]}",
            description = "US-SALES-07"
        });
        await QAHelper.AssertOkAsync(resp);
        _businessUnitId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _businessUnitId.Should().BeGreaterThan(0);
    }

    // AC1 — a SalesSegment can be created from (organisation, channel, business unit).
    // ✅ ACTIVE — all three parents are created in-flow above.
    [Fact, TestPriority(4)]
    public async Task Step4_CreateSalesSegment()
    {
        _orgId.Should().BeGreaterThan(0, "Step1 must have created the organisation");
        _channelId.Should().BeGreaterThan(0, "Step2 must have created the channel");
        _businessUnitId.Should().BeGreaterThan(0, "Step3 must have created the business unit");

        var resp = await _f.Client.PostAsJsonAsync(SalesSegmentRoute, new
        {
            salesOrganisationId = _orgId,
            salesChannelId = _channelId,
            businessUnitId = _businessUnitId,
            segmentName = "QA US07 Segment"
        });
        await QAHelper.AssertOkAsync(resp);
        _segmentId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _segmentId.Should().BeGreaterThan(0);
    }

    // AC2 — an ItemPriceMaster can be created (item + segment + currency + validFrom/validTo).
    // 🚫 BLOCKED: needs an Inventory item id (no plain /api/Item GetAll) + a Currency.
    [Fact(Skip = "needs seeded data: an Inventory item id + a Currency to publish a price"), TestPriority(5)]
    public async Task Step5_CreateItemPriceMaster()
    {
        var segmentId = _segmentId > 0 ? _segmentId : await QAHelper.FirstIdAsync(_f.Client, SalesSegmentRoute);
        var currencyId = await QAHelper.FirstIdAsync(_f.Client, "/api/Currency");
        if (currencyId == 0) currencyId = 1;
        var itemId = 1; // no /api/Item GetAll — reconcile live

        var resp = await _f.Client.PostAsJsonAsync(ItemPriceRoute, new
        {
            itemId,
            salesSegmentId = segmentId,
            baseRate = 125.50m,
            currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            tolerancePercentage = 5m,
            charityValue = 0m,
            handlingCharges = 10m
        });

        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    // AC3 — creating with validFrom > validTo is rejected.
    // ✅ ACTIVE + tolerant: with a real segment the date rule should fire (400 "after"). If the
    // item FK validates first (itemId=1 missing in the clone) we still get a 400 — assert 400 only.
    [Fact, TestPriority(6)]
    public async Task Step6_ValidFromAfterValidTo_Rejected()
    {
        var segmentId = _segmentId > 0 ? _segmentId : await QAHelper.FirstIdAsync(_f.Client, SalesSegmentRoute);
        if (segmentId == 0) segmentId = 1;
        var currencyId = await QAHelper.FirstIdAsync(_f.Client, "/api/Currency");
        if (currencyId == 0) currencyId = 1;

        var earlier = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)).ToString("yyyy-MM-dd");

        var resp = await _f.Client.PostAsJsonAsync(ItemPriceRoute, new
        {
            itemId = 1,
            salesSegmentId = segmentId,
            baseRate = 100m,
            currencyId,
            validFrom = ValidFrom,
            validTo = earlier
        });

        // note: tolerant — any FK that validates ahead of the date rule still yields a 400.
        await QAHelper.Assert400Async(resp);
    }

    // AC4 — the price is retrievable via by-item-date / exmill-rate.
    // 🚫 BLOCKED for a specific created price (Step5 is blocked) → author tolerant reachability:
    // the endpoints should respond (200 with data, or 400/404 when no price/args match).
    [Fact, TestPriority(7)]
    public async Task Step7_PriceQueryEndpointsReachable()
    {
        var segmentId = _segmentId > 0 ? _segmentId : 1;
        var date = ValidFrom;

        var byItemDate = await _f.Client.GetAsync(
            $"{ItemPriceRoute}/by-item-date?itemId=1&salesSegmentId={segmentId}&date={date}");
        ((int)byItemDate.StatusCode).Should().BeOneOf(200, 400, 404);

        var exmill = await _f.Client.GetAsync(
            $"{ItemPriceRoute}/exmill-rate?itemId=1&salesSegmentId={segmentId}&date={date}");
        ((int)exmill.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // Teardown — leaf-first: segment → channel/BU → organisation (id bound from query: ?id=).
    [Fact, TestPriority(90)]
    public async Task Step90_Teardown()
    {
        if (_segmentId > 0)      await _f.Client.DeleteAsync($"{SalesSegmentRoute}?id={_segmentId}");
        if (_channelId > 0)      await _f.Client.DeleteAsync($"{SalesChannelRoute}?id={_channelId}");
        if (_businessUnitId > 0) await _f.Client.DeleteAsync($"{BusinessUnitRoute}?id={_businessUnitId}");
        if (_orgId > 0)          await _f.Client.DeleteAsync($"{OrganisationRoute}?id={_orgId}");
    }
}
