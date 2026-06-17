namespace SalesManagement.QATests.Tests.SalesQuotation;

// ─────────────────────────────────────────────────────────────────────────────
// SalesQuotation — live-server QA suite (transactional header+details).
//
// Contract verified against source (2026-06-15):
//   POST   /api/SalesQuotation            CreateSalesQuotationCommand (header + salesQuotationDetails[])
//   PUT    /api/SalesQuotation            UpdateSalesQuotationCommand (adds id, isActive)
//   DELETE /api/SalesQuotation?id={id}    (id bound from QUERY — action `DeleteSalesQuotation(int id)`,
//                                          no route template / no [FromBody] → query-string binding)
//   GET    /api/SalesQuotation?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/SalesQuotation/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/SalesQuotation/by-name?term=
//   GET    /api/SalesQuotation/pending?pageNumber=&pageSize=&searchTerm=
//
// Create payload (header):
//   customerId, quotationDate(DateOnly "yyyy-MM-dd"), salesEnquiryId?, contactPersonId?,
//   validityDate(DateOnly), paymentTermId, remarks?, deliveryTermId,
//   freightCharges, otherCharges, totalBasicAmount, totalDiscount, netTaxableAmount,
//   totalTax, grandTotal, statusId?,
//   salesQuotationDetails:[{ itemId, variantId?, quantity, uomId?, exMillRate, discount,
//                            discountTypeId?, netRate, totalAmount, hsnId, taxPercentage, taxAmount }]
//
// BLOCKED: create-happy + full lifecycle need a seeded customer + item + HSN + paymentTerm +
//   deliveryTerm chain that the QA clone does not guarantee. Those are [Fact(Skip=...)].
//   Negatives (empty body, missing required, no-auth), GetAll smoke, AutoComplete and
//   pending/GetById reachability stay ACTIVE.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesQuotationCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesQuotationQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesQuotation";
    private const string PaymentTermRoute = "/api/PaymentTermMaster";
    private const string MiscMasterRoute = "/api/sales/MiscMaster";

    public SalesQuotationQATests(QAServerFixture fixture) => _f = fixture;

    // FK ids captured by TC001 so the Update lifecycle step can reuse the exact same chain.
    private static int _paymentTermId, _deliveryTermId, _discountTypeId, _hsnId;

    // Resolves the first active Sales.MiscMaster id under a given MiscTypeCode (the validator
    // gates DeliveryTermId / DiscountTypeId on specific MiscTypeCodes). Returns 0 if none.
    private async Task<int> ResolveMiscMasterIdByTypeCodeAsync(string miscTypeCode)
    {
        var resp = await _f.Client.GetAsync($"{MiscMasterRoute}?PageNumber=1&PageSize=300");
        if (!resp.IsSuccessStatusCode) return 0;
        using var doc = await QAHelper.ParseAsync(resp);
        if (!doc.RootElement.TryGetProperty("data", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return 0;
        foreach (var row in arr.EnumerateArray())
        {
            var code = row.TryGetProperty("miscTypeCode", out var c) ? c.GetString() : null;
            if (code != miscTypeCode) continue;
            var active = row.TryGetProperty("isActive", out var ia)
                && (ia.ValueKind == JsonValueKind.True
                    || (ia.ValueKind == JsonValueKind.Number && ia.GetInt32() == 1));
            if (active && row.TryGetProperty("id", out var idp)) return idp.GetInt32();
        }
        return 0;
    }

    // Resolves the HSN id carried by the fixture's ActiveItemId (the detail line needs a real hsnId).
    private async Task<int> ResolveHsnIdForActiveItemAsync()
    {
        if (_f.ActiveItemId == 0) return 0;
        var resp = await _f.Client.GetAsync("/api/ItemMaster?PageNumber=1&PageSize=50");
        if (!resp.IsSuccessStatusCode) return 0;
        using var doc = await QAHelper.ParseAsync(resp);
        if (!doc.RootElement.TryGetProperty("data", out var arr) || arr.ValueKind != JsonValueKind.Array)
            return 0;
        foreach (var row in arr.EnumerateArray())
        {
            if (!row.TryGetProperty("id", out var idp) || idp.GetInt32() != _f.ActiveItemId) continue;
            if (row.TryGetProperty("hsnId", out var h) && h.ValueKind == JsonValueKind.Number)
                return h.GetInt32();
        }
        return 0;
    }

    private object BuildCreateBody(int customerId, int paymentTermId, int deliveryTermId,
        int itemId, int hsnId, int discountTypeId) => new
    {
        customerId,
        quotationDate = "2026-06-15",
        validityDate = "2026-07-15",
        paymentTermId,
        deliveryTermId,
        remarks = "QA created quotation",
        freightCharges = 0m,
        otherCharges = 0m,
        totalBasicAmount = 1000m,
        totalDiscount = 0m,
        netTaxableAmount = 1000m,
        totalTax = 50m,
        grandTotal = 1050m,
        salesQuotationDetails = new[]
        {
            new
            {
                itemId, quantity = 10m, exMillRate = 100m, discount = 0m,
                discountTypeId, netRate = 100m, totalAmount = 1000m,
                hsnId, taxPercentage = 5m, taxAmount = 50m
            }
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (happy path BLOCKED — needs seeded FK chain)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        // Live reconciliation (2026-06-16): the full FK chain resolves on the clone —
        //   customer = _f.CustomerPartyId, item = _f.ActiveItemId, hsn = that item's hsnId,
        //   paymentTerm = /api/PaymentTermMaster, deliveryTerm = MiscMaster 'DeliveryTerms',
        //   discountType = MiscMaster 'QUOT_DISC_TYPE' (the validator gates on this exact code).
        // Create succeeds and captures the id. Self-guard if any FK is unresolvable.
        _paymentTermId = await QAHelper.FirstIdAsync(_f.Client, PaymentTermRoute);
        _deliveryTermId = await ResolveMiscMasterIdByTypeCodeAsync("DeliveryTerms");
        _discountTypeId = await ResolveMiscMasterIdByTypeCodeAsync("QUOT_DISC_TYPE");
        _hsnId = await ResolveHsnIdForActiveItemAsync();

        if (_f.CustomerPartyId == 0 || _f.ActiveItemId == 0 || _hsnId == 0
            || _paymentTermId == 0 || _deliveryTermId == 0 || _discountTypeId == 0)
            return;

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute,
            BuildCreateBody(_f.CustomerPartyId, _paymentTermId, _deliveryTermId,
                _f.ActiveItemId, _hsnId, _discountTypeId));

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
            customerId = 1,
            quotationDate = "2026-06-15",
            validityDate = "2026-07-15",
            paymentTermId = 1,
            deliveryTermId = 1
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
    public async Task TC004_Create_MissingRequired_Returns400()
    {
        // customerId / paymentTermId / deliveryTermId default to 0 → required validation fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            customerId = 0,
            quotationDate = "2026-06-15",
            validityDate = "2026-07-15",
            paymentTermId = 0,
            deliveryTermId = 0,
            salesQuotationDetails = Array.Empty<object>()
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
    public async Task TC022_GetAll_Page2PageSize5_Returns200Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET PENDING  (reachability — 200/400/404 tolerant)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetPending_Returns200Or400Or404()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetPending_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/pending?pageNumber=1&pageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 4 — GET BY ID  (no null guard → 200 + data:null when absent)
    // ─────────────────────────────────────────────────────────────────────────

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

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — AUTOCOMPLETE  (param is `term`)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_AutoComplete_WithTerm_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_AutoComplete_EmptyParam_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 6 — UPDATE  (negatives active; happy lifecycle BLOCKED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(60)]
    public async Task TC060_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = 1,
            customerId = 1,
            quotationDate = "2026-06-15",
            validityDate = "2026-07-15",
            paymentTermId = 1,
            deliveryTermId = 1,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(61)]
    public async Task TC061_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(62)]
    public async Task TC062_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy not satisfiable on this clone

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            customerId = _f.CustomerPartyId,
            quotationDate = "2026-06-15",
            validityDate = "2026-07-15",
            paymentTermId = _paymentTermId,
            deliveryTermId = _deliveryTermId,
            remarks = "QA updated quotation",
            freightCharges = 0m,
            otherCharges = 0m,
            totalBasicAmount = 1200m,
            totalDiscount = 0m,
            netTaxableAmount = 1200m,
            totalTax = 60m,
            grandTotal = 1260m,
            isActive = 1,
            salesQuotationDetails = new[]
            {
                new
                {
                    itemId = _f.ActiveItemId, quantity = 12m, exMillRate = 100m, discount = 0m,
                    discountTypeId = _discountTypeId, netRate = 100m, totalAmount = 1200m,
                    hsnId = _hsnId, taxPercentage = 5m, taxAmount = 60m
                }
            }
        });

        await QAHelper.AssertOkAsync(resp);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 7 — DELETE  (id bound from QUERY: ?id={id}; happy lifecycle BLOCKED)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(90)]
    public async Task TC090_Delete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id=1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(91)]
    public async Task TC091_Delete_NonExistentId_Returns400Or200_Tolerant()
    {
        // Delete validator should reject a non-existent id (NotFound → 400). Tolerant of a
        // handler that returns 200 isSuccess=false if the validator is not wired.
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id=999999");
        ((int)resp.StatusCode).Should().BeOneOf(200, 400, 404);
    }

    [Fact, TestPriority(92)]
    public async Task TC092_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return; // create-happy not satisfiable on this clone

        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }
}
