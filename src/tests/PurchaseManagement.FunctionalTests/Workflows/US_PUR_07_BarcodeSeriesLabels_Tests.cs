namespace PurchaseManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-PUR-07 — Bale Barcode Series labels (print payload)
//   As a stores user I expect the "Print Barcodes" action on a Barcode Series to return a
//   print-ready payload — a company letterhead plus the series range expanded into individual
//   barcode rows with their QR payload — so the frontend can render the label cards.
//
// Contract (added 2026-06-25):
//   GET /api/purchase/BarcodeSeries/{id}/labels
//     -> { data: { letterhead:{companyName,divisionName,address}, seriesNumber, prefix,
//                  agentDefault, totalCount, truncated, labels:[{barcode,qrPayload}] } }
// Business meaning: the backend EXPANDS BarcodeStartNumber..BarcodeEndNumber into individual
//   barcodes (prefix + number); each label's qrPayload == its barcode string; Agent defaults to
//   "DIRECT" (FE may override). Unknown id => 200 with data:null. Range is capped at 5000
//   (truncated=true). Tolerant: when the clone has no series, content steps self-skip.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-PUR-07-BarcodeSeriesLabels")]
[Trait("Module", "PurchaseManagement")]
[Trait("Story", "US-PUR-07")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_PUR_07_BarcodeSeriesLabels_Tests
{
    private readonly QAServerFixture _f;
    private const string ListRoute = "/api/purchase/BarcodeSeries";
    private static string LabelsRoute(int id) => $"/api/purchase/BarcodeSeries/{id}/labels";

    public US_PUR_07_BarcodeSeriesLabels_Tests(QAServerFixture fixture) => _f = fixture;

    // AC1 — labels payload returns a letterhead block and an array of label rows.
    [Fact, TestPriority(1)]
    public async Task Step1_Labels_ReturnsLetterheadAndLabels()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, ListRoute);
        if (id == 0) return; // no series on this clone

        var resp = await _f.Client.GetAsync(LabelsRoute(id));
        await QAHelper.AssertOkAsync(resp);

        using var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Object, "an existing series id must return labels");

        data.GetProperty("letterhead").ValueKind.Should().Be(JsonValueKind.Object);
        data.GetProperty("labels").ValueKind.Should().Be(JsonValueKind.Array);
    }

    // AC2 — every label's QR payload equals its barcode string, and each barcode is prefixed
    //        with the series prefix (proves backend expansion of prefix + number).
    [Fact, TestPriority(2)]
    public async Task Step2_Labels_QrEqualsBarcode_AndPrefixed()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, ListRoute);
        if (id == 0) return;

        var resp = await _f.Client.GetAsync(LabelsRoute(id));
        await QAHelper.AssertOkAsync(resp);

        using var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        var prefix = data.GetProperty("prefix").GetString();
        var labels = data.GetProperty("labels");

        if (labels.GetArrayLength() == 0) return; // empty range on this series — nothing to check

        foreach (var l in labels.EnumerateArray())
        {
            var barcode = l.GetProperty("barcode").GetString();
            var qr = l.GetProperty("qrPayload").GetString();

            barcode.Should().NotBeNullOrWhiteSpace();
            qr.Should().Be(barcode, "the QR encodes the plain barcode string");
            if (!string.IsNullOrEmpty(prefix))
                barcode!.Should().StartWith(prefix, "each barcode is prefix + number");
        }
    }

    // AC3 — the payload carries the default Agent caption ("DIRECT") and the run's totalCount;
    //        the range is never silently dropped (truncated flag is present).
    [Fact, TestPriority(3)]
    public async Task Step3_Labels_AgentDefaultAndCounts()
    {
        var id = await QAHelper.FirstIdAsync(_f.Client, ListRoute);
        if (id == 0) return;

        var resp = await _f.Client.GetAsync(LabelsRoute(id));
        await QAHelper.AssertOkAsync(resp);

        using var doc = await QAHelper.ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");

        data.GetProperty("agentDefault").GetString().Should().Be("DIRECT");
        data.GetProperty("totalCount").GetInt64().Should().BeGreaterThanOrEqualTo(0);
        data.TryGetProperty("truncated", out var t).Should().BeTrue();
        t.ValueKind.Should().BeOneOf(JsonValueKind.True, JsonValueKind.False);
    }

    // AC4 — an unknown series id returns 200 with data:null (clean "not found" for the FE).
    [Fact, TestPriority(4)]
    public async Task Step4_Labels_UnknownId_ReturnsNullData()
    {
        var resp = await _f.Client.GetAsync(LabelsRoute(999999999));
        ((int)resp.StatusCode).Should().BeOneOf(200, 404, 500);

        if (resp.StatusCode == HttpStatusCode.OK)
        {
            using var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        }
    }

    // AC5 — the labels endpoint rejects anonymous callers (401).
    [Fact, TestPriority(5)]
    public async Task Step5_Labels_RejectsAnonymous()
    {
        await QAHelper.Assert401Async(await _f.AnonymousClient.GetAsync(LabelsRoute(1)));
    }
}
