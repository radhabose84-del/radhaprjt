namespace SalesManagement.QATests.Tests.SalesEnquiry;

// ─────────────────────────────────────────────────────────────────────────────
// SalesEnquiry — live-server QA suite (transactional; full CRUD).
//
// Contract verified against source (2026-06-15):
//   CreateSalesEnquiryCommand wraps a single DTO property `SalesEnquiryDetails` (the header DTO),
//   which itself carries a nested `SalesEnquiryDetails` line-item array. The create body is:
//   POST   /api/SalesEnquiry
//          {
//            salesEnquiryDetails: {                 // header DTO wrapper
//              partyId,                             // cross-module FK (PartyManagement)
//              enquiryDate,                         // DateTimeOffset
//              enquiryTypeId,                       // FK
//              contactPerson?, expectedDeliveryDate?, paymentTermId?, salesLeadId?, remarks?,
//              salesEnquiryDetails: [               // nested line items
//                { itemId, variantId?, quantity, exmillRate?, targetPrice?, discount? }
//              ]
//            }
//          }
//   PUT    /api/SalesEnquiry
//          { id, partyId, enquiryDate, enquiryTypeId, contactPerson?, expectedDeliveryDate?,
//            paymentTermId?, salesLeadId?, remarks?, isActive, salesEnquiryDetails:[…] }   // FLAT (no wrapper)
//   DELETE /api/SalesEnquiry?id={id}               (id bound from QUERY — action param `int id`)
//   GET    /api/SalesEnquiry?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/SalesEnquiry/{id}                  (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/SalesEnquiry/by-name?term=
//
// FK / seeding note:
//   A valid create needs a real partyId (PartyManagement) + itemId (InventoryManagement); neither
//   is guaranteed in the QA clone, so create-happy and id-dependent lifecycle steps are
//   [Fact(Skip="needs seeded data: …")]. Negatives / Smoke / reachability stay ACTIVE.
//
// Create AND Update return a BARE int (IRequest<int>) → response `data` is a number.
//   CreatedId() tolerates both bare-number and object shapes.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesEnquiryCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesEnquiryQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesEnquiry";

    public SalesEnquiryQATests(QAServerFixture fixture) => _f = fixture;

    // Header DTO wrapper used by POST. partyId/enquiryTypeId/itemId left as supplied args.
    private static object BuildCreateBody(int partyId, int enquiryTypeId, int itemId) => new
    {
        salesEnquiryDetails = new
        {
            partyId,
            enquiryDate = "2026-06-15T00:00:00Z",
            enquiryTypeId,
            contactPerson = "QA Tester",
            remarks = "QA created enquiry",
            salesEnquiryDetails = new[]
            {
                new { itemId, variantId = (int?)null, quantity = 10m, exmillRate = (decimal?)null, targetPrice = (decimal?)null, discount = (decimal?)null }
            }
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "live blocker: EnquiryTypeExistsAsync requires a Sales.MiscMaster row under MiscTypeCode 'ENQ_TYPE', but the BannariERP_QATest clone has NO such MiscTypeMaster code (its enquiry types live under 'SalesEnquiryType' id 20). enquiryTypeId can never validate → create returns 400 'EnquiryTypeId ... inactive/deleted'. party (1113) + item (2260) resolve fine; this is a reference-data mismatch the QA seed does not cover"), TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildCreateBody(_f.CustomerPartyId, enquiryTypeId: 30, _f.ActiveItemId));

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        var id = doc.RootElement.CreatedId();
        id.Should().BeGreaterThan(0);
        _f.CreatedId = id;
    }

    [Fact, TestPriority(2)]
    public async Task TC002_Create_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute,
            BuildCreateBody(partyId: 1, enquiryTypeId: 1, itemId: 1));

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_MissingParty_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesEnquiryDetails = new
            {
                partyId = 0,
                enquiryDate = "2026-06-15T00:00:00Z",
                enquiryTypeId = 1,
                salesEnquiryDetails = new[] { new { itemId = 1, quantity = 10m } }
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_MissingLineItems_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesEnquiryDetails = new
            {
                partyId = 1,
                enquiryDate = "2026-06-15T00:00:00Z",
                enquiryTypeId = 1
            }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_InvalidFks_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildCreateBody(partyId: 999999, enquiryTypeId: 999999, itemId: 999999));

        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 2 — GET ALL
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(20)]
    [Trait("Layer", "Smoke")]
    public async Task TC020_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");

        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
        if (resp.StatusCode == HttpStatusCode.OK)
        {
            var doc = await QAHelper.ParseAsync(resp);
            doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
            doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
            doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
        }
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (no null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created SalesEnquiry (TC001 is skipped)"), TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetById_NonExistentId_Returns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/999999");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_AutoComplete_WithTerm_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (flat body; Update returns bare int → lifecycle Skipped)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact(Skip = "needs seeded data: a successfully created SalesEnquiry (TC001 is skipped)"), TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            partyId = 1,
            enquiryDate = "2026-06-15T00:00:00Z",
            enquiryTypeId = 1,
            contactPerson = "QA Updated",
            remarks = "QA updated enquiry",
            isActive = 1,
            salesEnquiryDetails = new[] { new { itemId = 1, quantity = 20m } }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            partyId = 1,
            enquiryDate = "2026-06-15T00:00:00Z",
            enquiryTypeId = 1,
            isActive = 1,
            salesEnquiryDetails = new[] { new { itemId = 1, quantity = 20m } }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_NonExistentId_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            partyId = 1,
            enquiryDate = "2026-06-15T00:00:00Z",
            enquiryTypeId = 1,
            isActive = 1,
            salesEnquiryDetails = new[] { new { itemId = 1, quantity = 20m } }
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — DELETE  (ALWAYS LAST — id bound from QUERY: ?id={id})
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_IdZero_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=0");
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(Skip = "needs seeded data: a successfully created SalesEnquiry (TC001 is skipped)"), TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact(Skip = "needs seeded data: a successfully created+deleted SalesEnquiry (TC001/TC093 are skipped)"), TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }
}
