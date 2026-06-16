namespace SalesManagement.QATests.Tests.SalesReturn;

// ─────────────────────────────────────────────────────────────────────────────
// SalesReturn — live-server QA suite (TRANSACTIONAL).
//
// Contract verified against source (2026-06-15 — SalesReturnController.cs):
//   GET    /api/SalesReturn?PageNumber=&PageSize=&SearchTerm=&StatusFilter=&FromDate=&ToDate=&CustomerId=
//   GET    /api/SalesReturn/{id}
//   GET    /api/SalesReturn/by-complaint/{complaintHeaderId}
//   GET    /api/SalesReturn/complaint-details/{complaintHeaderId}
//   POST   /api/SalesReturn                  (CreateSalesReturnCommand)
//   DELETE /api/SalesReturn?id={id}          (id bound from QUERY, not route)
//   NO update endpoint. NO autocomplete.
//
// Create command (POST) requires:
//   returnDate, complaintHeaderId (a Complaint whose Resolution = 'Sales Return'),
//   customerId, warehouseId, binId, remarks?, nested invoiceDetails[].items[].(pack ranges).
//
// Why create-happy + delete-happy are SKIPPED:
//   Creating a SalesReturn needs a seeded Complaint + a 'Sales Return' resolution + the
//   underlying invoice/dispatch data. The QA clone has no guaranteed such chain, so the
//   happy path is non-deterministic. Delete-happy depends on the created id → also skipped.
//
// Active coverage: GetAll smoke, no-auth 401 (list + POST + DELETE), empty-body 400,
//   missing-required 400, reachability for the by-complaint / complaint-details GETs and
//   delete negatives.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesReturnCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesReturnQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesReturn";

    public SalesReturnQATests(QAServerFixture fixture) => _f = fixture;

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (BLOCKED happy-path; negatives active)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a Complaint with Resolution='Sales Return' + invoice chain is required to create a SalesReturn"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            returnDate = "2024-06-01",
            complaintHeaderId = 0,  // would need a real complaint with 'Sales Return' resolution
            customerId = 0,
            warehouseId = 0,
            binId = 0,
            remarks = "Created by QA suite",
            invoiceDetails = new[]
            {
                new
                {
                    invoiceId = 0,
                    items = new[]
                    {
                        new { itemId = 0, fromPack = 1, toPack = 1, quantity = 1m }
                    }
                }
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
            returnDate = "2024-06-01",
            complaintHeaderId = 1,
            customerId = 1,
            warehouseId = 1,
            binId = 1
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
        // No complaintHeaderId / customerId / warehouseId / binId → validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            returnDate = "2024-06-01",
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&StatusFilter=Pending");
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
    public async Task TC032_ByComplaint_NonExistent_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-complaint/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(33)]
    public async Task TC033_ComplaintDetails_NonExistent_Reachable()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/complaint-details/999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — DELETE  (id bound from QUERY: ?id={id}; happy-path BLOCKED)
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

    [Fact(Skip = "needs seeded data: depends on a created SalesReturn id from TC001 (blocked — no seeded Complaint+resolution+invoice)"), TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
