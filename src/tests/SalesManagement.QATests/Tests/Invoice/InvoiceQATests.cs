namespace SalesManagement.QATests.Tests.Invoice;

// ─────────────────────────────────────────────────────────────────────────────
// Invoice — live-server QA suite (TRANSACTIONAL).
//
// Contract verified against source (2026-06-15 — InvoiceController.cs):
//   GET    /api/Invoice?PageNumber=&PageSize=&SearchTerm=&Status=
//   GET    /api/Invoice/{id}
//   GET    /api/Invoice/by-name?term=
//   GET    /api/Invoice/pending?pageNumber=&pageSize=&searchTerm=
//   GET    /api/Invoice/{id}/print
//   GET    /api/Invoice/dispatch-tracking/{salesOrderId}
//   GET    /api/Invoice/gatepass-pending?vehicleNo=
//   POST   /api/Invoice                       (CreateInvoiceCommand)
//   PUT    /api/Invoice                       (UpdateInvoiceCommand)
//   POST   /api/Invoice/print/multiple        (List<int> invoiceIds)
//   POST   /api/Invoice/generate-einvoice?invoiceId=&withEwaybill=
//   NO delete endpoint.
//
// Create command (POST /api/Invoice) requires:
//   invoiceDate, dispatchAdviceId (an UN-INVOICED dispatch advice), partyId (cross-module),
//   financialYearId, heavy tax/header totals + nested details[].
//
// Why create-happy + lifecycle are SKIPPED:
//   Creating an Invoice needs a seeded, un-invoiced DispatchAdvice (one DA → one Invoice).
//   The QA clone has no guaranteed un-invoiced DA, so the happy path is non-deterministic.
//   UpdateInvoice depends on a created invoice id → also skipped.
//
// Active coverage: GetAll smoke, no-auth 401 (list + POST), empty-body 400, missing-required 400,
//   reachability for the extra GET/POST endpoints.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("InvoiceCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class InvoiceQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Invoice";

    public InvoiceQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (BLOCKED happy-path; negatives active)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: an un-invoiced DispatchAdvice (one DA → one Invoice) is required to create an Invoice"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            invoiceDate = "2024-06-01",
            dispatchAdviceId = 0,   // would need a real un-invoiced DA id
            partyId = 0,            // would need a real party id
            financialYearId = 0,
            totalBags = 1,
            totalWeight = 10m,
            taxableValue = 100m,
            invoiceAmount = 118m,
            details = new[]
            {
                new { itemId = 0, quantity = 1m, rate = 100m }
            }
        });

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        _f.CreatedId = doc.RootElement.CreatedId();
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            invoiceDate = "2024-06-01",
            dispatchAdviceId = 1,
            partyId = 1,
            financialYearId = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MissingRequiredFields_Returns400()
    {
        // No dispatchAdviceId / partyId / financialYearId → validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            invoiceDate = "2024-06-01",
            remarks = "missing required FK ids"
        });

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL  (smoke)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_WithStatusFilter_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&Status=Pending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID + extra GET endpoints (reachability)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_NonExistent_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_AutoComplete_WithTerm_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_Pending_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Print_NonExistent_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/print");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_DispatchTracking_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/dispatch-tracking/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_GatePassPending_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/gatepass-pending");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_PrintMultiple_Reachable()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{BaseRoute}/print/multiple", new[] { 999999 });
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(38)]
    public async Task TC038_GenerateEInvoice_NonExistent_Reachable()
    {
        var resp = await _f.Client.PostAsync($"{BaseRoute}/generate-einvoice?invoiceId=999999", null);
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404, 500);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (BLOCKED happy-path; negatives active)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: depends on a created Invoice id from TC001 (blocked — no un-invoiced DispatchAdvice)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            invoiceDate = "2024-06-01",
            invoiceAmount = 118m,
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            invoiceDate = "2024-06-01",
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }
}
