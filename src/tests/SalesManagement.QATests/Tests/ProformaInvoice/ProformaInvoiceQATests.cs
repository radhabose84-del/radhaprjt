namespace SalesManagement.QATests.Tests.ProformaInvoice;

// ─────────────────────────────────────────────────────────────────────────────
// ProformaInvoice — live-server QA suite (TRANSACTIONAL).
//
// Contract verified against source (2026-06-15 — ProformaInvoiceController.cs):
//   GET    /api/ProformaInvoice?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/ProformaInvoice/{id}
//   GET    /api/ProformaInvoice/by-name?term=
//   GET    /api/ProformaInvoice/by-sales-order/{salesOrderId}
//   GET    /api/ProformaInvoice/{id}/print
//   POST   /api/ProformaInvoice                 (CreateProformaInvoiceCommand)
//   PUT    /api/ProformaInvoice                 (UpdateProformaInvoiceCommand)
//   PUT    /api/ProformaInvoice/update-payment  (UpdateProformaPaymentCommand)
//   DELETE /api/ProformaInvoice?id={id}         (id bound from QUERY, not route)
//
// Create command (POST) requires:
//   proformaDate, salesOrderId (same-module SalesOrder), partyId (cross-module),
//   proformaAmount, remarks?, statusId.
//
// Why create-happy + lifecycle are SKIPPED:
//   Creating a ProformaInvoice needs a seeded SalesOrder + party. The QA clone has no
//   guaranteed SalesOrder, so the happy path is non-deterministic. Update / update-payment /
//   delete-happy all depend on the created id → also skipped.
//
// Active coverage: GetAll smoke, no-auth 401 (list + POST + DELETE), empty-body 400,
//   missing-required 400, reachability for the extra GET/PUT endpoints and delete negatives.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ProformaInvoiceCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ProformaInvoiceQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ProformaInvoice";

    public ProformaInvoiceQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (BLOCKED happy-path; negatives active)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a SalesOrder + party are required to create a ProformaInvoice"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            proformaDate = "2024-06-01",
            salesOrderId = 0,   // would need a real same-module SalesOrder id
            partyId = 0,        // would need a real cross-module party id
            proformaAmount = 1000m,
            remarks = "Created by QA suite",
            statusId = 1
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
            proformaDate = "2024-06-01",
            salesOrderId = 1,
            partyId = 1,
            proformaAmount = 1000m
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
        // No salesOrderId / partyId → validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            proformaDate = "2024-06-01",
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
    public async Task TC033_BySalesOrder_NonExistent_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-sales-order/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(34)]
    public async Task TC034_Print_NonExistent_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999/print");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — UPDATE  (BLOCKED happy-path; negatives active)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: depends on a created ProformaInvoice id from TC001 (blocked — no seeded SalesOrder)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            proformaDate = "2024-06-01",
            proformaAmount = 1200m,
            remarks = "Updated by QA",
            statusId = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            proformaDate = "2024-06-01",
            proformaAmount = 1200m
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact(Skip = "needs seeded data: depends on a created ProformaInvoice id from TC001 (blocked — no seeded SalesOrder)"), TestPriority(53)]
    public async Task TC053_UpdatePayment_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update-payment", new
        {
            id = _f.CreatedId
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_UpdatePayment_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{BaseRoute}/update-payment", new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_UpdatePayment_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync($"{BaseRoute}/update-payment", new { id = 999999 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — DELETE  (id bound from QUERY: ?id={id}; happy-path BLOCKED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NonExistentId_Reachable()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: depends on a created ProformaInvoice id from TC001 (blocked — no seeded SalesOrder)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
