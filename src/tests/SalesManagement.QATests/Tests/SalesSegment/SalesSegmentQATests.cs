namespace SalesManagement.QATests.Tests.SalesSegment;

// ─────────────────────────────────────────────────────────────────────────────
// SalesSegment — live-server QA suite (CRUD lifecycle + negatives).
//
// Contract verified against source (2026-06-15):
//   POST   /api/SalesSegment            { salesOrganisationId, salesChannelId, businessUnitId,
//                                          currencyId?, validFrom?, segmentName }
//   PUT    /api/SalesSegment            { id, currencyId?, validFrom?, segmentName, isActive }
//   DELETE /api/SalesSegment?id={id}    (id bound from QUERY — DeleteSalesSegment(int id), no [FromBody])
//   GET    /api/SalesSegment?PageNumber=&PageSize=&SearchTerm=
//   GET    /api/SalesSegment/{id}       (returns 200 + data:null when not found — NO 404 guard)
//   GET    /api/SalesSegment/by-name?term=
//
// Key facts that shaped assertions:
//   • Composite UNIQUE key (salesOrganisationId + salesChannelId + businessUnitId); these 3
//     are IMMUTABLE and NOT present on the Update command.
//   • salesOrganisationId / salesChannelId / businessUnitId are required same-module FKs
//     (SalesOrganisationExistsAsync / SalesChannelExistsAsync / BusinessUnitExistsAsync).
//   • currencyId is an OPTIONAL cross-module FK (ICurrencyLookup) — only validated when > 0.
//   • Composite duplicate error message: "This combination of Sales Organisation, Sales
//     Channel, and Business Unit already exists."
//   • FK ids resolved at runtime via FirstIdAsync against /api/SalesOrganisation,
//     /api/SalesChannel, /api/BusinessUnit (the QA clone has no guaranteed seed ids).
//   • Reads are NOT company-scoped (WHERE IsDeleted = 0 only) → created row visible in GetAll.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("SalesSegmentCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class SalesSegmentQATests
{
    private readonly QAServerFixture _f;
    private const string BaseRoute = "/api/SalesSegment";

    private const string TestName = "QA Test Sales Segment";

    // Resolved-at-runtime FK ids for the composite key. Captured by TC001.
    private static int _salesOrgId;
    private static int _salesChannelId;
    private static int _businessUnitId;

    public SalesSegmentQATests(QAServerFixture fixture) => _f = fixture;

    // Resolve the 3 required parent FK ids (fallback 1 if a parent endpoint is empty).
    private async Task ResolveFksAsync()
    {
        if (_salesOrgId != 0) return; // resolve once per collection run
        _salesOrgId = await QAHelper.FirstIdAsync(_f.Client, "/api/SalesOrganisation");
        _salesChannelId = await QAHelper.FirstIdAsync(_f.Client, "/api/SalesChannel");
        if (_salesOrgId == 0) _salesOrgId = 1;
        if (_salesChannelId == 0) _salesChannelId = 1;

        // FIX (test bug, reconciled 2026-06-16): the composite UNIQUE key is (org, channel, bu).
        // The first existing BusinessUnit already has a segment in the clone's seed data, so the
        // first-available triple is never free. Mint a fresh, run-unique BusinessUnit so the
        // composite is guaranteed new — makes the happy-path create re-runnable without a reset.
        var buCode = _f.EntityCode[..Math.Min(20, _f.EntityCode.Length)];
        var buResp = await _f.Client.PostAsJsonAsync("/api/BusinessUnit", new
        {
            businessUnitCode = buCode,
            businessUnitName = $"QA Seg BU {_f.EntityCode[..Math.Min(8, _f.EntityCode.Length)]}",
            description = "Created by SalesSegment QA suite"
        });
        if (buResp.IsSuccessStatusCode)
        {
            var buDoc = await QAHelper.ParseAsync(buResp);
            _businessUnitId = buDoc.RootElement.CreatedId();
        }
        if (_businessUnitId == 0)
            _businessUnitId = await QAHelper.FirstIdAsync(_f.Client, "/api/BusinessUnit");
        if (_businessUnitId == 0) _businessUnitId = 1;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 1 — CREATE  (TC001 captures CreatedId)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    public async Task TC001_Create_HappyPath_Returns200_And_CapturesId()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = TestName
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
        await ResolveFksAsync();

        var resp = await _f.AnonymousClient.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = "No Auth Segment"
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
    public async Task TC004_Create_SegmentNameEmpty_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = ""
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task TC005_Create_SalesOrganisationIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = 0,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = TestName
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task TC006_Create_SalesChannelIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = 0,
            businessUnitId = _businessUnitId,
            segmentName = TestName
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(7)]
    public async Task TC007_Create_BusinessUnitIdMissing_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = 0,
            segmentName = TestName
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(8)]
    public async Task TC008_Create_SegmentNameTooLong_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = new string('A', 201) // exceeds SegmentName max (200)
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "longer than");
    }

    [Fact, TestPriority(9)]
    public async Task TC009_Create_NonExistentSalesOrganisation_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = 999999,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = TestName
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(10)]
    public async Task TC010_Create_NonExistentCurrency_Returns400()
    {
        await ResolveFksAsync();

        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            currencyId = 999999,
            segmentName = TestName
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_Create_DuplicateComposite_Returns400()
    {
        await ResolveFksAsync();

        // Same composite key (salesOrgId + salesChannelId + businessUnitId) as TC001.
        var resp = await _f.Client.PostAsJsonAsync(BaseRoute, new
        {
            salesOrganisationId = _salesOrgId,
            salesChannelId = _salesChannelId,
            businessUnitId = _businessUnitId,
            segmentName = "Duplicate Composite Segment"
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "exists");
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
    public async Task TC022_GetAll_SearchByName_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=1&PageSize=15&SearchTerm={TestName}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(23)]
    public async Task TC023_GetAll_Page2PageSize5_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}?PageNumber=2&PageSize=5");
        ((int)resp.StatusCode).Should().BeOneOf(200, 404);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 3 — GET BY ID  (controller has NO null guard → 200 + data:null)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetById_ValidId_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{BaseRoute}/{_f.CreatedId}");

        await QAHelper.AssertOkAsync(resp);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.GetProperty("data").GetProperty("id").GetInt32()
            .Should().Be(_f.CreatedId);
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
        var resp = await _f.Client.GetAsync($"{BaseRoute}/by-name?term=QA");

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
        var resp = await _f.AnonymousClient.GetAsync($"{BaseRoute}/by-name?term=QA");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SECTION 5 — UPDATE  (composite key fields are immutable — not in update command)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_Update_HappyPath_Returns200()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            segmentName = "QA Updated Segment",
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
            segmentName = "QA Updated Segment",
            isActive = 1
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(52)]
    public async Task TC052_Update_SegmentNameEmpty_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            segmentName = "",
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(53)]
    public async Task TC053_Update_IsActiveInvalid_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            segmentName = "QA Updated Segment",
            isActive = 2
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(54)]
    public async Task TC054_Update_NonExistentId_Returns400_NotFound()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 999999,
            segmentName = "QA Updated Segment",
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "not found");
    }

    [Fact, TestPriority(55)]
    public async Task TC055_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = 0,
            segmentName = "QA Updated Segment",
            isActive = 1
        });

        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(56)]
    public async Task TC056_Update_Inactivate_Then_Reactivate_Returns200()
    {
        var inactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            segmentName = "QA Updated Segment",
            isActive = 0
        });
        await QAHelper.AssertOkAsync(inactivate);

        var reactivate = await _f.Client.PutAsJsonAsync(BaseRoute, new
        {
            id = _f.CreatedId,
            segmentName = "QA Updated Segment",
            isActive = 1
        });
        await QAHelper.AssertOkAsync(reactivate);
    }

    [Fact, TestPriority(57)]
    public async Task TC057_Update_EmptyBody_Returns400()
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
        var resp = await _f.AnonymousClient.DeleteAsync($"{BaseRoute}?id={_f.CreatedId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(93)]
    public async Task TC093_Delete_HappyPath_SoftDelete_Returns200()
    {
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
