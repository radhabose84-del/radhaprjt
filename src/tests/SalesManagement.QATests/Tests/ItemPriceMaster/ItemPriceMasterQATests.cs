namespace SalesManagement.QATests.Tests.ItemPriceMaster;

// ─────────────────────────────────────────────────────────────────────────────
// ItemPriceMaster — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/ItemPriceMaster            { itemId, variantId?, salesSegmentId, baseRate,
//                                            currencyId, validFrom (DateOnly "yyyy-MM-dd"),
//                                            validTo (DateOnly, > validFrom), tolerancePercentage?,
//                                            charityValue?, handlingCharges?, statusId? }
//   PUT    /api/ItemPriceMaster            { id, itemId, variantId?, salesSegmentId, baseRate,
//                                            currencyId, validFrom, validTo, ...optionals, isActive }
//   DELETE /api/ItemPriceMaster?id={id}    (id bound from QUERY — [HttpDelete] Delete(int id), no route param)
//   GET    /api/ItemPriceMaster?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/ItemPriceMaster/{id}       (NO null guard → 200 + data:null when not found)
//   GET    /api/ItemPriceMaster/by-name?term=
//
// Key facts that shaped assertions:
//   • Required (NotEmpty): ItemId, SalesSegmentId, CurrencyId, ValidFrom, ValidTo.
//   • ValidTo MUST be > ValidFrom  → "Valid To must be after Valid From."
//   • FK checks: ItemExistsAsync (Inventory, cross-module), SalesSegmentExistsAsync
//     (same-module → /api/SalesSegment), CurrencyExistsAsync (UserManagement → /api/Currency).
//   • No unique CODE; composite OVERLAP uniqueness on (Item, Variant, Segment, date range).
//   • No DateOnly serialization helper — DateOnly serializes to ISO "yyyy-MM-dd" by default.
//
// FK resolution (QA clone has no guaranteed seed id = 1):
//   • salesSegmentId → FirstIdAsync(/api/SalesSegment) (same-module master, reliably listable).
//   • currencyId     → FirstIdAsync(/api/Currency).
//   • itemId         → there is no plain /api/Item GetAll endpoint; resolution falls back to 1.
//     Create-happy therefore depends on item id 1 existing in the clone — RECONCILE LIVE if it
//     fails with an ItemId FK message (the negative/validation tests do not depend on this).
// ─────────────────────────────────────────────────────────────────────────────

[Collection("ItemPriceMasterCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class ItemPriceMasterQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/ItemPriceMaster";

    // Resolved-at-runtime FK ids (set in TC001). Fallback 1 when a lookup is unreachable/empty.
    private static int _itemId = 1;
    private static int _salesSegmentId = 1;
    private static int _currencyId = 1;

    // Date window — ISO "yyyy-MM-dd"; ValidTo strictly after ValidFrom.
    // FIX (test bug, reconciled 2026-06-22): a fixed far-future window dodged the clone's seed
    // prices but NOT this suite's own row from an earlier run — OverlapExistsAsync only checks
    // (Item, Segment, Variant, overlapping dates), so a second run without a DB reset collided
    // ("An active price record already exists."). Derive a RUN-UNIQUE far-future window from the
    // run-unique EntityCode: CONSTANT within a run (so TC001 creates and TC010 still collides on
    // the same window) but DIFFERENT across runs → fully re-runnable without a reset.
    private int RunOffsetDays() => int.Parse(_f.EntityCode[1..7]) % 60000;
    private string ValidFrom => new DateOnly(2090, 1, 1).AddDays(RunOffsetDays()).ToString("yyyy-MM-dd");
    private string ValidTo => new DateOnly(2090, 1, 1).AddDays(RunOffsetDays() + 30).ToString("yyyy-MM-dd");

    public ItemPriceMasterQATests(QAServerFixture fixture) => _f = fixture;

    private async Task ResolveFksAsync()
    {
        var seg = await QAHelper.FirstIdAsync(_f.Client, "/api/SalesSegment");
        if (seg > 0) _salesSegmentId = seg;

        var cur = await QAHelper.FirstIdAsync(_f.Client, "/api/Currency");
        if (cur > 0) _currencyId = cur;

        // Use the real ACTIVE Inventory item id resolved by the shared fixture from the live clone.
        if (_f.ActiveItemId > 0) _itemId = _f.ActiveItemId;
    }

    private object ValidCreateBody() => new
    {
        itemId = _itemId,
        salesSegmentId = _salesSegmentId,
        baseRate = 125.50m,
        currencyId = _currencyId,
        validFrom = ValidFrom,
        validTo = ValidTo,
        tolerancePercentage = 5m,
        charityValue = 0m,
        handlingCharges = 10m
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    // Un-skipped 2026-06-16: item FK resolved via _f.ActiveItemId AND the Sales 'PriceMaster'
    // document-numbering TransactionType is now provided by qa-seed.ps1. Self-skips on a bare,
    // un-seeded clone (no active item resolvable).
    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        if (_f.ActiveItemId == 0) return;   // self-skip on a bare clone with no resolvable active item
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidCreateBody());

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
            itemId = 1,
            salesSegmentId = 1,
            baseRate = 100m,
            currencyId = 1,
            validFrom = ValidFrom,
            validTo = ValidTo
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Create_ItemIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = 0,
            salesSegmentId = _salesSegmentId,
            baseRate = 100m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(4)]
    public async Task TC004_Create_SalesSegmentIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = _itemId,
            salesSegmentId = 0,
            baseRate = 100m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_CurrencyIdMissing_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 100m,
            currencyId = 0,
            validFrom = ValidFrom,
            validTo = ValidTo
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_ValidFromMissing_Returns400()
    {
        // Omit validFrom → default(DateOnly) "0001-01-01" → NotEqual(default) fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 100m,
            currencyId = _currencyId,
            validTo = ValidTo
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_ValidToBeforeValidFrom_Returns400()
    {
        var earlier = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)).ToString("yyyy-MM-dd");

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 100m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = earlier
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "after");
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_NonExistentSalesSegment_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = _itemId,
            salesSegmentId = 999999,
            baseRate = 100m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NonExistentCurrency_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 100m,
            currencyId = 999999,
            validFrom = ValidFrom,
            validTo = ValidTo
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_DuplicateOverlap_Returns400()
    {
        // Same Item + Segment + overlapping date range as TC001 → OverlapExistsAsync fails.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, ValidCreateBody());

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new { });
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

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
        doc.RootElement.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
        doc.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        doc.RootElement.GetProperty("pageSize").GetInt32().Should().Be(15);
    }

    [Fact, TestPriority(21)]
    public async Task TC021_GetAll_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(22)]
    public async Task TC022_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200_WithData()
    {
        if (_f.CreatedId == 0) return;   // TC001 create didn't run on this clone
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetById_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/{_f.CreatedId}");
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=A");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(41)]
    public async Task TC041_AutoComplete_EmptyParam_Returns200_Array()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(42)]
    public async Task TC042_AutoComplete_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=A");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        if (_f.CreatedId == 0) return;   // TC001 create didn't run on this clone
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            tolerancePercentage = 7m,
            isActive = 1
        });

        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(51)]
    public async Task TC051_Update_NoAuthToken_Returns401()
    {
        var resp = await _f.AnonymousClient.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_ValidToBeforeValidFrom_Returns400()
    {
        var earlier = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)).ToString("yyyy-MM-dd");

        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = earlier,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_EmptyBody_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new { });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        if (_f.CreatedId == 0) return;   // TC001 create didn't run on this clone
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            itemId = _itemId,
            salesSegmentId = _salesSegmentId,
            baseRate = 175.00m,
            currencyId = _currencyId,
            validFrom = ValidFrom,
            validTo = ValidTo,
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
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
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
        if (_f.CreatedId == 0) return;   // TC001 create didn't run on this clone
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.AssertOkAsync(resp);
    }

    [Fact, TestPriority(94)]
    public async Task TC094_Delete_AlreadyDeleted_Returns400()
    {
        var resp = await _f.Client.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(95)]
    public async Task TC095_VerifySoftDelete_GetByIdReturns200_WithNullData()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }
}
