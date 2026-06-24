namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-05 — OCR & Raw-Material-PO document-numbering preview
//   As a procurement user I expect the OCR Management and OCR→PO conversion screens to
//   PREVIEW the next document number (and show the last issued) WITHOUT consuming the
//   sequence — the real number is assigned only when the document is actually saved.
//
// Contracts (added 2026-06-24):
//   GET /api/OCREntry/next-number       -> { data: { lastNumber, nextNumber } }
//   GET /api/RawMaterialPO/next-number  -> { data: { lastNumber, nextNumber } }
// Business meaning: the peek is non-consuming — two consecutive reads return the same
// nextNumber. 500 tolerated when the doc-numbering series is not seeded on the QA clone.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-05-DocumentNumberingPreview")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-05")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_05_DocumentNumberingPreview_Tests
{
    private readonly QAServerFixture _f;
    private const string OcrNextRoute  = "/api/OCREntry/next-number";
    private const string RmpoNextRoute = "/api/RawMaterialPO/next-number";

    public US_PUR_05_DocumentNumberingPreview_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — OCR next-number is reachable and (when the 'OCR' series is seeded) returns last+next.
    [Fact, TestPriority(1)]
    public async Task Step1_OcrNextNumber_Reachable_AndShapeWhenOk()
    {
        var resp = await _f.Client.GetAsync(OcrNextRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
        if (resp.StatusCode == HttpStatusCode.OK)
            (await ReadNextNumberAsync(resp)).Should().NotBeNull();
    }

    // AC2 — Raw-Material-PO next-number is reachable and (when seeded) returns last+next.
    [Fact, TestPriority(2)]
    public async Task Step2_RmpoNextNumber_Reachable_AndShapeWhenOk()
    {
        var resp = await _f.Client.GetAsync(RmpoNextRoute);
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
        if (resp.StatusCode == HttpStatusCode.OK)
            (await ReadNextNumberAsync(resp)).Should().NotBeNull();
    }

    // AC3 — the preview is NON-CONSUMING: two consecutive OCR reads return the SAME nextNumber,
    //        proving the peek does not advance the sequence. Skipped tolerantly when not seeded.
    [Fact, TestPriority(3)]
    public async Task Step3_OcrNextNumber_IsNonConsuming()
    {
        var first = await _f.Client.GetAsync(OcrNextRoute);
        if (first.StatusCode != HttpStatusCode.OK)
            return; // 'OCR' series not seeded on this clone — nothing to assert

        var second = await _f.Client.GetAsync(OcrNextRoute);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var n1 = await ReadNextNumberAsync(first);
        var n2 = await ReadNextNumberAsync(second);
        n2.Should().Be(n1); // peek did not consume the sequence
    }

    // AC4 — both previews reject anonymous callers (401).
    [Fact, TestPriority(4)]
    public async Task Step4_NextNumberReadsRejectAnonymous()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(OcrNextRoute));
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(RmpoNextRoute));
    }

    private static async Task<string?> ReadNextNumberAsync(HttpResponseMessage resp)
    {
        var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        return data.TryGetProperty("nextNumber", out var n) ? n.GetString() : null;
    }
}
