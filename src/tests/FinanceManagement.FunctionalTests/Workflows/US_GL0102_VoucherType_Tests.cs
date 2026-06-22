namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL01-02 — Voucher Type Configuration (workflow / story).
//
//   As a System Administrator, I configure voucher types each with their own dedicated
//   number series, allowed account types and number padding, so finance can introduce new
//   document types without a code change.
//
// Proves the CONFIGURATION story (QA covers individual endpoints): create → own series →
// edit → FY reset → deactivate. FK ids (an AccountTypeMaster + a FinancialYear) are resolved
// at runtime from the clone. The saved-voucher TYPE-LOCK (AC3) needs the voucher-entry
// transaction table (separate feature) and is [Fact(Skip=…)] — never a silent gap.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL01-02")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL01-02")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0102_VoucherType_Tests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/VoucherTypeMaster";
    private const int CompanyId = 1;

    private static int _id;
    private static string _code = string.Empty;
    private static int _accountTypeId;
    private static int _financialYearId;

    public US_GL0102_VoucherType_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    // STEP 1 (AC4) — a created voucher type is immediately available, no deployment / code change.
    [Fact, TestPriority(1)]
    public async Task Step1_CreateVoucherType_BecomesAvailable_NoDeployment()
    {
        _accountTypeId = await QAHelper.FirstIdAsync(_f.Client, $"/api/finance/accounttypemaster?CompanyId={CompanyId}");
        _financialYearId = await QAHelper.FirstIdAsync(_f.Client, "/api/FinancialYear");
        _accountTypeId.Should().BeGreaterThan(0, "the clone must have a seeded AccountTypeMaster row (Asset, …)");
        _financialYearId.Should().BeGreaterThan(0, "the clone must have a seeded FinancialYear");

        _code = $"VT{new string(_f.EntityCode.Where(char.IsLetterOrDigit).Take(6).ToArray())}";

        var createResp = await _f.Client.PostAsJsonAsync(Route, new
        {
            // companyId + financialYearId are derived server-side (token + current FY) — not sent.
            voucherTypeCode = _code,
            voucherTypeName = $"VT Story {_f.EntityCode}",
            numberPadding = 4,
            allowedAccountTypeIds = new[] { _accountTypeId }
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        _id = (await ParseAsync(createResp)).RootElement.GetProperty("data").GetInt32();
        _id.Should().BeGreaterThan(0);

        // Available to the selectable list (autocomplete) with no code change.
        var byName = await _f.Client.GetAsync($"{Route}/by-name?term={_code}");
        byName.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = (await ParseAsync(byName)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("voucherTypeCode").GetString());
        codes.Should().Contain(_code, "a new voucher type is selectable immediately, no deployment");
    }

    // STEP 2 (AC1) — the new type generates numbers from its OWN dedicated series, starting at /0001.
    [Fact, TestPriority(2)]
    public async Task Step2_NewType_HasOwnSeries_StartingAtOne()
    {
        _id.Should().BeGreaterThan(0, "Step1 must have created it");

        var resp = await _f.Client.GetAsync($"{Route}/number-series?FinancialYearId={_financialYearId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var row = (await ParseAsync(resp)).RootElement.GetProperty("data").EnumerateArray()
            .FirstOrDefault(x => x.GetProperty("voucherTypeCode").GetString() == _code);
        row.ValueKind.Should().Be(JsonValueKind.Object, "the new type must have a series row for the fiscal year");
        row.GetProperty("nextNumber").GetString().Should().EndWith("/0001",
            "a brand-new type's dedicated series starts at 0001");
    }

    // STEP 3 — edit name + padding (code is immutable).
    [Fact, TestPriority(3)]
    public async Task Step3_EditVoucherType()
    {
        _id.Should().BeGreaterThan(0);

        var upd = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            voucherTypeName = $"VT Story Edited {_f.EntityCode}",
            numberPadding = 5,
            isActive = 1,
            allowedAccountTypeIds = new[] { _accountTypeId }
        });
        upd.StatusCode.Should().Be(HttpStatusCode.OK);

        var byId = await _f.Client.GetAsync($"{Route}/{_id}");
        var data = (await ParseAsync(byId)).RootElement.GetProperty("data");
        data.GetProperty("voucherTypeName").GetString().Should().Be($"VT Story Edited {_f.EntityCode}");
        data.GetProperty("voucherTypeCode").GetString().Should().Be(_code, "code is immutable");
    }

    // STEP 4 (AC2) — resetting the series returns the next number to /000…1.
    [Fact, TestPriority(4)]
    public async Task Step4_ResetSeries_ReturnsToOne()
    {
        _id.Should().BeGreaterThan(0);

        var reset = await _f.Client.PostAsJsonAsync($"{Route}/reset-series", new
        {
            voucherTypeId = _id,
            financialYearId = _financialYearId
        });
        reset.StatusCode.Should().Be(HttpStatusCode.OK);

        var resp = await _f.Client.GetAsync($"{Route}/number-series?FinancialYearId={_financialYearId}&CompanyId={CompanyId}");
        var row = (await ParseAsync(resp)).RootElement.GetProperty("data").EnumerateArray()
            .First(x => x.GetProperty("voucherTypeCode").GetString() == _code);
        row.GetProperty("lastUsedNumber").GetInt32().Should().Be(0, "reset sets the counter back to 0");
        row.GetProperty("nextNumber").GetString().Should().EndWith("1");
    }

    // STEP 5 (Deactivate) — inactive type drops out of the selectable list but stays in GetAll.
    [Fact, TestPriority(5)]
    public async Task Step5_Deactivate_ExcludesFromList_ButKeepsInGetAll()
    {
        _id.Should().BeGreaterThan(0);

        var deact = await _f.Client.PutAsJsonAsync(Route, new
        {
            id = _id,
            voucherTypeName = $"VT Story Edited {_f.EntityCode}",
            numberPadding = 5,
            isActive = 0,
            allowedAccountTypeIds = new[] { _accountTypeId }
        });
        deact.StatusCode.Should().Be(HttpStatusCode.OK);

        var byName = await _f.Client.GetAsync($"{Route}/by-name?term={_code}");
        var listCodes = (await ParseAsync(byName)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("voucherTypeCode").GetString());
        listCodes.Should().NotContain(_code, "deactivated types are hidden from the selectable list");

        var all = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=200&SearchTerm={_code}");
        var allCodes = (await ParseAsync(all)).RootElement.GetProperty("data").EnumerateArray()
            .Select(x => x.GetProperty("voucherTypeCode").GetString());
        allCodes.Should().Contain(_code, "deactivation is not deletion — record is retained (IsDeleted=0)");
    }

    // STEP 6 (AC3) — type lock on a saved voucher depends on the voucher-entry transaction table.
    [Fact(Skip = "Blocked — AC3 (saved-voucher type lock) needs the voucher-entry transaction table " +
                 "(separate feature). On this master the delete/code-change guard for consumed series is " +
                 "covered by QA + integration; the posted-voucher lock lands with voucher entry."),
     TestPriority(6)]
    public async Task Step6_SavedVoucherTypeLock_IsVoucherEntry()
    {
        await Task.CompletedTask;
    }
}
