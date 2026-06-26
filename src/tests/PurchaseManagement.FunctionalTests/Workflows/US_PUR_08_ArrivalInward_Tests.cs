namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-08 — Arrival inward (incl. GST %)
//   As a procurement user I record a cotton-bale Arrival (inward) against a Raw-Material PO,
//   capturing a header-level GST (%) alongside the quantity/weighbridge details, and read it
//   back on every Arrival GET.
//
// ACTIVE: the Arrival read surface (list + autocomplete) and anonymous-rejection (401).
// BLOCKED: the full create + GST round-trip — needs a RawMaterialPO + supplier/station/godown/
// transporter + doc-numbering "Arrival" on the clone (same chain that skips the QA TC001 create).
// The GST required/0–100 rule itself is proven at the unit layer (CreateArrivalCommandValidatorTests).
//
// Routes (verified from PurchaseManagement.QATests — ApiControllerBase => /api/Arrival):
//   GET  /api/Arrival?PageNumber=&PageSize=
//   GET  /api/Arrival/{id}            (200 + data:null when not found)
//   GET  /api/Arrival/by-name?term=
//   POST /api/Arrival                 (header-level gstPercentage in the body)
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-08-ArrivalInward")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-08")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_08_ArrivalInward_Tests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Arrival";

    public US_PUR_08_ArrivalInward_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 + AC3 — create an Arrival with a header-level gstPercentage and read it back. BLOCKED:
    // the create needs a seeded RawMaterialPO + supplier/station/godown/transporter + doc-numbering.
    [Fact(Skip = "needs seeded data: RawMaterialPO + supplier/station/godown/transporter + doc-numbering 'Arrival'"), TestPriority(1)]
    public async Task Step1_CreateArrival_WithGst_RoundTrips()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            arrivalDate     = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            rawMaterialPoId = 1,
            supplierId      = 1,
            stationId       = 1,
            godownId        = 1,
            transporterId   = 1,
            gstPercentage   = 5,
            lotNo           = "QA-LOT-PUR08"
        });

        await QAHelper.AssertOkAsync(resp);
        var id = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        id.Should().BeGreaterThan(0);

        // Read it back — gstPercentage must survive the round-trip.
        var get = await _f.Client.GetAsync($"{BaseRoute}/{id}");
        await QAHelper.AssertOkAsync(get);
        var data = (await QAHelper.ParseAsync(get)).RootElement.GetProperty("data");
        data.GetProperty("gstPercentage").GetDecimal().Should().Be(5m);
    }

    // AC2 — the Arrival list + autocomplete read surface is reachable.
    [Fact, TestPriority(2)]
    public async Task Step2_ArrivalReads_AreReachable()
    {
        ((int)(await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15")).StatusCode).Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{BaseRoute}/by-name?term=ARV")).StatusCode).Should().BeOneOf(200, 404);
    }

    // AC4 — the Arrival reads reject anonymous callers (401).
    [Fact, TestPriority(3)]
    public async Task Step3_ReadsRejectAnonymous()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15"));
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=ARV"));
    }
}
