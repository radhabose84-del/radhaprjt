namespace FinanceManagement.QATests.Tests.CurrencyForexConfig;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-12 — Currency / Forex Configuration master (live-server QA).
//
// Route: api/finance/CurrencyForexConfig
//   GET (list, paged+search) · GET {id} · GET by-name (autocomplete)
//   POST · PUT · DELETE {id}   (CompanyId resolved from the token; CurrencyTypeCode immutable+alphanumeric)
//
// Fully self-seeding (no FK/MiscMaster dependencies) → the whole CRUD lifecycle runs live.
// Only the Rule-25 "delete blocked when a GL account references it" case needs a seeded GL
// account and is documented as a Skip until one is resolvable in the QA clone.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("CurrencyForexConfigCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CurrencyForexConfigQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/CurrencyForexConfig";

    public CurrencyForexConfigQATests(QAServerFixture fixture) => _f = fixture;

    // CurrencyTypeCode must be alphanumeric (no hyphen/space), unique per company AND <= 20 chars.
    // EntityCode is ~20 letters/digits, so "QA"+EntityCode overflows the 20-char column → truncate.
    private string UniqueCode() =>
        $"QA{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(18).ToArray())}";

    private object ValidCreateBody(string code) => new
    {
        currencyTypeCode = code,
        currencyTypeName = $"QA Type {_f.EntityCode}"
    };

    // ── SMOKE — login → auth → DB → read works (deploy gate) ──────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task CFC001_GetAll_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=15");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = await QAHelper.ParseAsync(resp);
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH — anonymous requests rejected ────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task CFC002_GetAll_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=15");
        await QAHelper.Assert401Async(resp);
    }

    [Fact, TestPriority(3)]
    public async Task CFC003_Create_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.PostAsJsonAsync($"{Route}", ValidCreateBody(UniqueCode()));
        await QAHelper.Assert401Async(resp);
    }

    // ── VALIDATION negatives ──────────────────────────────────────────────────
    [Fact, TestPriority(4)]
    public async Task CFC004_Create_MissingRequired_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{Route}", new { currencyTypeCode = "", currencyTypeName = "" });

        await QAHelper.Assert400Async(resp);
        await QAHelper.AssertBodyContainsAsync(resp, "required");
    }

    [Fact, TestPriority(5)]
    public async Task CFC005_Create_NonAlphanumericCode_Returns400()
    {
        var resp = await _f.Client.PostAsJsonAsync($"{Route}",
            new { currencyTypeCode = "QA-01", currencyTypeName = "Bad Code" });
        await QAHelper.Assert400Async(resp);
    }

    [Fact, TestPriority(6)]
    public async Task CFC006_Update_IdZero_Returns400()
    {
        var resp = await _f.Client.PutAsJsonAsync($"{Route}", new { id = 0, currencyTypeName = "", isActive = 1 });
        await QAHelper.Assert400Async(resp);
    }

    // ── FULL CRUD LIFECYCLE (self-seeding, runs live) ─────────────────────────
    [Fact, TestPriority(10)]
    public async Task CFC010_FullLifecycle()
    {
        var code = UniqueCode();

        // Create
        var createResp = await _f.Client.PostAsJsonAsync($"{Route}", ValidCreateBody(code));
        await QAHelper.AssertOkAsync(createResp);
        var id = await QAHelper.GetCreatedIdAsync(createResp);
        id.Should().BeGreaterThan(0);

        // Duplicate code → 400
        var dupResp = await _f.Client.PostAsJsonAsync($"{Route}", ValidCreateBody(code));
        await QAHelper.Assert400Async(dupResp);

        // GET by id
        (await _f.Client.GetAsync($"{Route}/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        // by-name autocomplete contains it (active)
        var byNameResp = await _f.Client.GetAsync($"{Route}/by-name?term={code}");
        byNameResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await QAHelper.ParseAsync(byNameResp)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("currencyTypeCode").GetString());
        codes.Should().Contain(code);

        // Update (rename)
        var updResp = await _f.Client.PutAsJsonAsync($"{Route}",
            new { id, currencyTypeName = $"QA Edited {_f.EntityCode}", isActive = 1 });
        await QAHelper.AssertOkAsync(updResp);

        // Deactivate (IsActive=0) → hidden from autocomplete, kept in GetAll
        var deactResp = await _f.Client.PutAsJsonAsync($"{Route}",
            new { id, currencyTypeName = $"QA Edited {_f.EntityCode}", isActive = 0 });
        await QAHelper.AssertOkAsync(deactResp);

        var byName2 = await _f.Client.GetAsync($"{Route}/by-name?term={code}");
        var codes2 = (await QAHelper.ParseAsync(byName2)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("currencyTypeCode").GetString());
        codes2.Should().NotContain(code);

        // Delete (soft) — unreferenced, succeeds
        var delResp = await _f.Client.DeleteAsync($"{Route}?id={id}");
        await QAHelper.AssertOkAsync(delResp);
    }

    // ── RULE 25 — delete blocked when a GL account references the config ──────
    [Fact(Skip = "Needs seeded data: a GL account referencing the config via GlAccountMaster.CurrencyTypeId. " +
                 "During live reconciliation, resolve/create a GL account that uses a CurrencyForexConfig id, then " +
                 "DELETE that config id → expect 400 'linked with other records'."),
     TestPriority(20)]
    public async Task CFC020_Delete_Blocked_When_Referenced_By_GlAccount()
    {
        await Task.CompletedTask;
    }
}
