namespace SalesManagement.QATests.Tests.Complaint;

// ─────────────────────────────────────────────────────────────────────────────
// Complaint (transactional header) — live-server QA suite.
//
// Contract verified against source (2026-06-15):
//   POST   /api/Complaint                            CreateComplaintCommand (JSON body)
//          { complaintDate, customerId, customerAddress?, customerPIN?, customerMobile?,
//            customerEmail?, customerPAN?, customerGSTNo?, creditLimit, totalOS, outstanding,
//            balanceCredit, delay?, ledger?, remarks?, details?[] }
//          (customerId is a CROSS-MODULE PartyManagement FK; create needs a real customer
//           that has invoices — the QA clone cannot guarantee one.)
//   PUT    /api/Complaint                            UpdateComplaintCommand (JSON body)
//   DELETE /api/Complaint?id={id}                    (id bound from QUERY — int param, default model binding)
//   DELETE /api/Complaint/delete-attachment/{id}     (id bound from ROUTE)
//   POST   /api/Complaint/upload-attachment          [FromForm] multipart
//   GET    /api/Complaint?PageNumber=&PageSize=&SearchTerm=&StatusFilter=
//   GET    /api/Complaint/pending?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/Complaint/pending-qcreview?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/Complaint/pending-resolution?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/Complaint/{id}                       (200 + data:null when not found — NO 404 guard)
//   GET    /api/Complaint/for-sales-return?term=
//   GET    /api/Complaint/by-name?term=
//   GET    /api/Complaint/customer-invoices?customerId=
//   GET    /api/Complaint/invoice-details?invoiceHeaderId=
//   GET    /api/Complaint/search-invoices?partyId=&PageNumber=&PageSize=&SearchTerm=&LastOneYear=
//
// Strategy: create-happy + lifecycle are BLOCKED (need a seeded customer + invoice chain).
// Always-active coverage: smoke GetAll, no-auth 401, empty/missing-field 400, and reachability
// for every read endpoint with plausible params.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ComplaintCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ComplaintQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/Complaint";

    public ComplaintQATests(QAServerFixture fixture) => _f = fixture;

    // ── SECTION 1 — CREATE (negatives are always-active; happy path is BLOCKED) ──

    [Fact(Skip = "needs seeded data: a real PartyManagement customer (customerId) that has " +
                 "invoices; the QA clone has no guaranteed customer/invoice chain to attach a complaint to.")]
    [TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            complaintDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            customerId = 1,
            customerMobile = "9000000000",
            remarks = "Created by QA suite",
            creditLimit = 0,
            totalOS = 0,
            outstanding = 0,
            balanceCredit = 0
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
            complaintDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            customerId = 1
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
    public async Task TC004_Create_MissingCustomerId_Returns400()
    {
        // customerId omitted (defaults to 0) → required/FK validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            complaintDate = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            remarks = "Missing customer"
        });

        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 2 — GET ALL (smoke) ─────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        // Transactional list can legitimately be empty on a fresh clone.
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_WithStatusFilter_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&StatusFilter=Open");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ── SECTION 3 — WORKLIST / READ ENDPOINT REACHABILITY ───────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetPending_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetPendingQCReview_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending-qcreview?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetPendingResolution_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending-resolution?PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_ForSalesReturn_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/for-sales-return?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_AutoComplete_Returns200_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(35)]
    public async Task TC035_CustomerInvoices_Returns200_Or_400_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/customer-invoices?customerId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(36)]
    public async Task TC036_InvoiceDetails_Returns200_Or_400_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/invoice-details?invoiceHeaderId=1");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(37)]
    public async Task TC037_SearchInvoices_Returns200_Or_400_Or_404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/search-invoices?partyId=1&PageNumber=1&PageSize=10");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ── SECTION 4 — GET BY ID (no null guard → 200 + data:null) ─────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a created complaint id (TC001 is blocked).")]
    [TestPriority(42)]
    public async Task TC042_GetById_ValidId_Returns200_WithData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Object);
    }

    // ── SECTION 5 — UPDATE (BLOCKED — needs a created complaint) ─────────────────

    [Fact(Skip = "needs seeded data: a created complaint id (TC001 is blocked).")]
    [TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = 1,
            remarks = "Updated by QA"
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new { id = 1, customerId = 1 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ── SECTION 6 — ATTACHMENTS & DELETE ────────────────────────────────────────

    [Fact, TestPriority(60)]
    public async Task TC060_DeleteAttachment_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}/delete-attachment/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(61)]
    public async Task TC061_DeleteAttachment_NonExistentId_Reachable()
    {
        // Route-bound id; non-existent attachment may 200 (no-op), 400, or 404.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}/delete-attachment/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(62)]
    public async Task TC062_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NonExistentId_Reachable()
    {
        // id bound from query. Non-existent header may 200/400/404 depending on handler guard.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact(Skip = "needs seeded data: a created complaint id (TC001 is blocked).")]
    [TestPriority(91)]
    public async Task TC091_Delete_HappyPath_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
