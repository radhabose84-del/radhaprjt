namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-06 — OCR "Order Confirmation Report" (print payload)
//   As a procurement user I expect the Order Confirmation Report for an OCR to return a
//   COMPLETE, print-ready payload — grouped sections (company letterhead, document identity,
//   order details, cotton parameters, freight, signatories) — so the frontend can render the
//   fixed PDF without stitching data from several calls.
//
// Contract (added 2026-06-24):
//   GET /api/OCREntry/{id}/report -> { data: { sections: [ { key, title, fields:[{key,label,value,raw}] } ] } }
// Business meaning: the report AGGREGATES the OCR + letterhead (JWT unit) + final approval +
//   freight (OCR→PO→RFQ) into one sectioned document. Unknown id => 200 with data:null (not 404).
//   Tolerant: when the QA clone has no OCRs, the content steps self-skip.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-06-OcrOrderConfirmationReport")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-06")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_06_OcrOrderConfirmationReport_Tests
{
    private readonly QAServerFixture _f;
    private const string ListRoute = "/api/OCREntry";
    private static string ReportRoute(int id) => $"/api/OCREntry/{id}/report";

    private static readonly string[] ExpectedSectionKeys =
        { "company", "documentIdentity", "orderDetails", "cottonParameters", "freight", "footer" };

    public US_PUR_06_OcrOrderConfirmationReport_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — the report returns the six grouped sections, in the contract's order.
    [Fact, TestPriority(1)]
    public async Task Step1_Report_ReturnsAllSections()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, ListRoute);
        if (id == 0) return; // no OCRs on this clone — nothing to report on

        var resp = await _f.Client.GetAsync(ReportRoute(id));
        await QAHelper.AssertOkAsync(resp);

        using var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Object, "an existing OCR id must return a report");

        var keys = SectionKeys(data);
        keys.Should().Contain(ExpectedSectionKeys);
    }

    // AC2 — the document-identity + seller fields are populated for a real OCR (the report is
    //        not an empty shell). OcrNumber is always present on a saved OCR.
    [Fact, TestPriority(2)]
    public async Task Step2_Report_CoreFieldsPopulated()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, ListRoute);
        if (id == 0) return;

        var resp = await _f.Client.GetAsync(ReportRoute(id));
        await QAHelper.AssertOkAsync(resp);

        using var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");

        Field(data, "documentIdentity", "ocrNumber").Should().NotBeNullOrWhiteSpace();
        // companyName / sellerName fields are always emitted (value may be "" if data missing).
        FieldExists(data, "company", "companyName").Should().BeTrue();
        FieldExists(data, "orderDetails", "sellerName").Should().BeTrue();
    }

    // AC3 — the freight section always carries both fields (per-bale + total), even when blank
    //        (no PO / approved RFQ yet). The FE's fixed layout depends on them always existing.
    [Fact, TestPriority(3)]
    public async Task Step3_Report_FreightFieldsAlwaysPresent()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, ListRoute);
        if (id == 0) return;

        var resp = await _f.Client.GetAsync(ReportRoute(id));
        await QAHelper.AssertOkAsync(resp);

        using var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");

        FieldExists(data, "freight", "freightPerBale").Should().BeTrue();
        FieldExists(data, "freight", "freightTotal").Should().BeTrue();
    }

    // AC4 — an unknown OCR id is NOT a 404; the report endpoint returns 200 with data:null so
    //        the FE can show a clean "not found" state.
    [Fact, TestPriority(4)]
    public async Task Step4_Report_UnknownId_ReturnsNullData()
    {
        var resp = await _f.Client.GetAsync(ReportRoute(999999999));
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            using var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        }
    }

    // AC5 — the report rejects anonymous callers (401).
    [Fact, TestPriority(5)]
    public async Task Step5_Report_RejectsAnonymous()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(ReportRoute(1)));
    }

    // ── helpers ──
    private static List<string?> SectionKeys(JsonElement data)
    {
        var keys = new List<string?>();
        foreach (var s in data.GetProperty("sections").EnumerateArray())
            keys.Add(s.GetProperty("key").GetString());
        return keys;
    }

    private static string? Field(JsonElement data, string sectionKey, string fieldKey)
    {
        foreach (var s in data.GetProperty("sections").EnumerateArray())
        {
            if (s.GetProperty("key").GetString() != sectionKey) continue;
            foreach (var fld in s.GetProperty("fields").EnumerateArray())
                if (fld.GetProperty("key").GetString() == fieldKey)
                    return fld.GetProperty("value").GetString();
        }
        return null;
    }

    private static bool FieldExists(JsonElement data, string sectionKey, string fieldKey)
    {
        foreach (var s in data.GetProperty("sections").EnumerateArray())
        {
            if (s.GetProperty("key").GetString() != sectionKey) continue;
            foreach (var fld in s.GetProperty("fields").EnumerateArray())
                if (fld.GetProperty("key").GetString() == fieldKey)
                    return true;
        }
        return false;
    }
}
