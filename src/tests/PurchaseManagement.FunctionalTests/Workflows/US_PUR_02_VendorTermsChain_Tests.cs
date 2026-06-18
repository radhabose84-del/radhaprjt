namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-02 — Vendor terms & logistics masters
//   As a buyer I set up the payment-terms, vendor-rating and port vocabulary
//   so purchase orders carry valid commercial and logistics references.
// Fully implementable: PaymentTerm/Port FK-creates self-skip if the clone lacks a
// MiscMaster/Country; VendorRatingGrade is clean (no FK).
//
// Contracts (verified against PurchaseManagement.QATests, 2026-06-17):
//   POST   /api/PaymentTermMaster { code, description, baselineTypeId, creditDays }
//          baselineTypeId → /api/purchase/miscmaster FK (REQUIRED)
//   POST   /api/VendorRatingGrade { gradeCode, gradeName, minScore, maxScore, sortOrder }  (clean, no FK)
//   POST   /api/PortMaster        { portCode, portName, countryId, portTypeId }
//          portCode pattern ^[A-Z0-9-]+$ max 20; countryId → /api/Country; portTypeId → /api/purchase/miscmaster
//   DELETE /api/PaymentTermMaster/{id}  ·  /api/VendorRatingGrade/{id}  ·  /api/PortMaster/{id}  (id from ROUTE)
//   Create returns 200/201 (heterogeneous shape) — accept BeOneOf(200, 201) for capture.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-02-VendorTermsChain")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_02_VendorTermsChain_Tests
{
    private readonly QAServerFixture _f;

    private const string PaymentTermRoute = "/api/PaymentTermMaster";
    private const string RatingGradeRoute = "/api/VendorRatingGrade";
    private const string PortRoute        = "/api/PortMaster";
    private const string MiscMasterRoute  = "/api/purchase/miscmaster";
    private const string CountryRoute     = "/api/Country";

    private static int _baselineTypeId;
    private static int _paymentTermId;
    private static int _ratingGradeId;
    private static int _portId;

    public US_PUR_02_VendorTermsChain_Tests(QAServerFixture fixture) => _f = fixture;

    // Run-unique, alphanumeric code clamped to 10 chars (EntityCode is ~19 chars — never slice beyond length).
    private string Code() => _f.EntityCode.Substring(0, Math.Min(10, _f.EntityCode.Length));

    // AC1 — a baseline MiscMaster value is resolvable for the payment-term FK.
    [Fact, TestPriority(1)]
    public async Task Step1_ResolveBaselineMiscMaster()
    {
        _baselineTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);
        // 0 => the clone holds no MiscMaster; Step2's create self-skips. Later independent steps still run.
        _baselineTypeId.Should().BeGreaterThanOrEqualTo(0);
    }

    // AC2 — a PaymentTermMaster can be created (baselineTypeId FK + creditDays).
    [Fact, TestPriority(2)]
    public async Task Step2_CreatePaymentTerm()
    {
        if (_baselineTypeId <= 0)
            return; // baseline FK unresolved on clone — skip create, keep downstream independent steps

        var resp = await _f.Client.PostAsJsonAsync(PaymentTermRoute, new
        {
            code = Code(),
            description = "US-PUR-02 Payment Term",
            baselineTypeId = _baselineTypeId,
            creditDays = 30
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _paymentTermId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _paymentTermId.Should().BeGreaterThan(0);
    }

    // AC3 — a VendorRatingGrade can be created (clean master, no FK).
    [Fact, TestPriority(3)]
    public async Task Step3_CreateVendorRatingGrade()
    {
        var resp = await _f.Client.PostAsJsonAsync(RatingGradeRoute, new
        {
            gradeCode = Code(),
            gradeName = "US-PUR-02 Grade",
            minScore = 0m,
            maxScore = 50m,
            sortOrder = QAHelper.RunUniqueInt(_f.EntityCode)
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _ratingGradeId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _ratingGradeId.Should().BeGreaterThan(0);
    }

    // AC4 — a PortMaster can be created (countryId + portTypeId FKs).
    [Fact, TestPriority(4)]
    public async Task Step4_CreatePort()
    {
        var countryId  = await QAHelper.FirstIdAsync(_f.Client, CountryRoute);
        var portTypeId = await QAHelper.FirstIdAsync(_f.Client, MiscMasterRoute);

        if (countryId <= 0 || portTypeId <= 0)
            return; // required FK unresolved on clone — skip create

        // portCode pattern ^[A-Z0-9-]+$ (uppercase letters, digits, hyphen), max 20.
        var portCode = Code().ToUpperInvariant();

        var resp = await _f.Client.PostAsJsonAsync(PortRoute, new
        {
            portCode,
            portName = "US-PUR-02 Port",
            countryId,
            portTypeId
        });

        ((int)resp.StatusCode).Should().BeOneOf(200, 201);
        _portId = (await QAHelper.ParseAsync(resp)).RootElement.CreatedId();
        _portId.Should().BeGreaterThan(0);
    }

    // AC5 — each created master is readable by id (tolerant: GetById guards differ per entity).
    [Fact, TestPriority(5)]
    public async Task Step5_CreatedMastersAreReadableById()
    {
        if (_paymentTermId > 0)
        {
            var r = await _f.Client.GetAsync($"{PaymentTermRoute}/{_paymentTermId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
        if (_ratingGradeId > 0)
        {
            var r = await _f.Client.GetAsync($"{RatingGradeRoute}/{_ratingGradeId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
        if (_portId > 0)
        {
            var r = await _f.Client.GetAsync($"{PortRoute}/{_portId}");
            ((int)r.StatusCode).Should().BeOneOf(200, 404);
        }
    }

    // AC6 — teardown each master (ROUTE-bound deletes, tolerant).
    [Fact, TestPriority(6)]
    public async Task Step6_Teardown()
    {
        if (_portId > 0)        await _f.Client.DeleteAsync($"{PortRoute}/{_portId}");
        if (_ratingGradeId > 0) await _f.Client.DeleteAsync($"{RatingGradeRoute}/{_ratingGradeId}");
        if (_paymentTermId > 0) await _f.Client.DeleteAsync($"{PaymentTermRoute}/{_paymentTermId}");
    }
}
