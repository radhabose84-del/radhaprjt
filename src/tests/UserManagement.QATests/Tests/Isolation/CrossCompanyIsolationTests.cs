namespace UserManagement.QATests.Tests.Isolation;

// ─────────────────────────────────────────────────────────────────────────────
// Phase 3(b) — Security: cross-company data-isolation (IDOR).
//
// Proves a user in Company A cannot read or modify Company B's data (and vice-versa).
// Uses TwoCompanyFixture, which authenticates two users on two companies.
//
// SKIPPED until a second company-bound user is provisioned. To enable:
//   1. Create two users on two companies in the QA DB.
//   2. Set QAServer:Isolation:CompanyA/CompanyB (Username/Password/CompanyId) in
//      appsettings.QA.json (or env QAServer__Isolation__CompanyA__Username, …).
//   3. Remove the Skip below.
//
// ⚠ First-run tuning expected (same loop as every live slice this session): the exact
//   company-scoped endpoint and the isolation response (404 vs empty vs 403) get
//   confirmed against real behavior. A genuine red here may be a REAL isolation gap,
//   not a test bug.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("IsolationCollection")]
[Trait("Layer", "Security")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class CrossCompanyIsolationTests
{
    private readonly TwoCompanyFixture _f;

    private const string DivisionRoute = "/api/Division";

    private const string SkipReason =
        "IDOR — requires two company-bound users. Set QAServer:Isolation:CompanyA/CompanyB " +
        "(Username/Password/CompanyId) in appsettings.QA.json (or env QAServer__Isolation__…), " +
        "then remove this Skip. Assertions may need first-run tuning to the actual " +
        "company-scoped endpoint/response.";

    public CrossCompanyIsolationTests(TwoCompanyFixture fixture) => _f = fixture;

    // CONTROL — Company A creates a row and can see it in its own scoped list.
    [Fact(Skip = SkipReason), TestPriority(1)]
    public async Task CompanyA_CreatesDivision_AndCanSeeItInOwnList()
    {
        var token = _f.EntityCode;

        var resp = await _f.ClientA.PostAsJsonAsync(DivisionRoute, new
        {
            shortName = _f.EntityCode[..6],
            name      = $"QA ISO {token}",
            companyId = _f.CompanyAId
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _f.CreatedId = (await Parse(resp)).CreatedId();
        _f.CreatedId.Should().BeGreaterThan(0);

        var list = await _f.ClientA.GetAsync(
            $"{DivisionRoute}?PageNumber=1&PageSize=100&SearchTerm={Uri.EscapeDataString(token)}");
        (await list.Content.ReadAsStringAsync())
            .Should().Contain(token, "Company A must see its own division");
    }

    // ISOLATION — Company B's scoped list must NOT contain Company A's row.
    [Fact(Skip = SkipReason), TestPriority(2)]
    public async Task CompanyB_CannotSee_CompanyA_DivisionInList()
    {
        var token = _f.EntityCode;

        var list = await _f.ClientB.GetAsync(
            $"{DivisionRoute}?PageNumber=1&PageSize=100&SearchTerm={Uri.EscapeDataString(token)}");
        list.StatusCode.Should().Be(HttpStatusCode.OK);

        (await list.Content.ReadAsStringAsync())
            .Should().NotContain(token,
                "Company B must NOT see Company A's division — cross-company data isolation");
    }

    // ISOLATION (write) — Company B must not be able to modify Company A's row.
    [Fact(Skip = SkipReason), TestPriority(3)]
    public async Task CompanyB_CannotUpdate_CompanyA_Division()
    {
        var resp = await _f.ClientB.PutAsJsonAsync(DivisionRoute, new
        {
            id        = _f.CreatedId,
            shortName = _f.EntityCode[..6],
            name      = "Hijacked by Company B",
            companyId = _f.CompanyBId,
            isActive  = (byte)1
        });

        // Expect rejection (not-found under B's scope, or forbidden). A 200 here would be a
        // REAL isolation gap worth raising, not a test failure to "fix".
        ((int)resp.StatusCode).Should().BeOneOf(403, 404);
    }

    private static async Task<JsonDocument> Parse(HttpResponseMessage r)
        => JsonDocument.Parse(await r.Content.ReadAsStringAsync());
}
