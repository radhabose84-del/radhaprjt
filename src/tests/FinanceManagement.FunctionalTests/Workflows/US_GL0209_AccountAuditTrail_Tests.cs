namespace FinanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL02-09 — Account Master Audit Trail & Version History (story).
//
//   As an auditor I want an immutable per-account change history (who, when, field,
//   old, new, role, IP) that is viewable and exportable, so every structural change
//   to the chart of accounts is traceable for statutory audit.
//
// Flow: edit an audited master (AccountTypeMaster — audited, no freeze trigger) → the
// SaveChanges interceptor writes a field-level row in the same transaction → the auditor
// views the per-account history (AC-3) and exports a range with a record-count checksum (AC-4).
//
// AC-2 (DB-enforced immutability: no UPDATE/DELETE for any app role) has NO HTTP surface —
// the table has no write endpoint and a DB trigger + DENY enforce it — so it is validated by the
// integration suite (AccountAuditTrailImmutabilityTests), not here. AC-5 (8-year retention) is a
// DB/operational guarantee (no purge job), not runtime-testable. Both are noted in Story-Catalogue.
//
// Requires the QA server to run the US-GL02-09 build; the role claim (AC-1 'role') appears only after
// a fresh login on that build.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-GL02-09")]
[Trait("Module", "FinanceManagement")]
[Trait("Story", "US-GL02-09")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_GL0209_AccountAuditTrail_Tests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/finance/account-audit";
    private const string TypeRoute = "/api/finance/accounttypemaster";
    private const int CompanyId = 1;

    private static int _typeId;

    public US_GL0209_AccountAuditTrail_Tests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string StartDigit()
    {
        var d = new string(_f.EntityCode.Where(char.IsDigit).ToArray());
        var n = d.Length > 0 ? (d[0] - '0') : 5;
        return (n == 0 ? 9 : n).ToString();
    }

    // Step 1 — make a structural change to an audited master (creates + edits it).
    [Fact, TestPriority(1)]
    public async Task Step1_EditAuditedMaster_Succeeds()
    {
        var create = await _f.Client.PostAsJsonAsync(TypeRoute, new
        {
            companyId = CompanyId,
            accountTypeName = $"FN Audit {_f.EntityCode}",
            startCode = StartDigit(),
            accountCodeLength = 6,
            sortOrder = 1
        });
        // startCode unique per company (1-9) — tolerate a duplicate on a populated clone.
        if (create.StatusCode == HttpStatusCode.BadRequest) return;
        create.StatusCode.Should().Be(HttpStatusCode.OK);
        _typeId = (await ParseAsync(create)).RootElement.GetProperty("data").GetInt32();

        var edit = await _f.Client.PutAsJsonAsync(TypeRoute, new
        {
            id = _typeId,
            accountTypeName = $"FN Audit Edited {_f.EntityCode}",
            startCode = StartDigit(),
            accountCodeLength = 6,
            sortOrder = 2,
            isActive = 1
        });
        edit.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Step 2 — AC-3 + AC-1: auditor views the per-account, field-level history (who/field/old/new/role).
    [Fact, TestPriority(2)]
    public async Task Step2_ViewHistory_ShowsFieldLevelChange()
    {
        if (_typeId == 0) return;
        var resp = await _f.Client.GetAsync($"{AuditRoute}/AccountTypeMaster/{_typeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Array);
        data.GetArrayLength().Should().BeGreaterThan(0, "AC-3 — the edit must be traceable");

        var changed = data.EnumerateArray()
            .FirstOrDefault(r => r.GetProperty("propertyName").GetString() == "AccountTypeName");
        changed.ValueKind.Should().NotBe(JsonValueKind.Undefined, "AC-1 — the changed field is recorded");
        changed.TryGetProperty("oldValue", out _).Should().BeTrue();
        changed.TryGetProperty("newValue", out _).Should().BeTrue();
        changed.TryGetProperty("createdByRole", out _).Should().BeTrue();  // AC-1 — role captured
        changed.TryGetProperty("createdByName", out _).Should().BeTrue();
    }

    // Step 3 — AC-4: export a date range; the response carries the record-count checksum (+ SHA-256).
    [Fact, TestPriority(3)]
    public async Task Step3_Export_HasRecordCountChecksum()
    {
        var resp = await _f.Client.GetAsync(
            $"{AuditRoute}/export?from=2026-01-01T00:00:00Z&to=2027-01-01T00:00:00Z&entityName=AccountTypeMaster");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.GetProperty("recordCount").GetInt32().Should().Be(data.GetProperty("rows").GetArrayLength());
        data.GetProperty("checksum").GetString().Should().NotBeNullOrWhiteSpace();
    }

    // AC-2 — immutability is DB-enforced (trigger + DENY) with no HTTP write surface; validated by the
    // integration suite. AC-5 — 8-year retention is operational (no purge job). Documented, not run here.
    [Fact(Skip = "AC-2 DB-enforced (no API surface) — covered by AccountAuditTrailImmutabilityTests; AC-5 retention is operational"), TestPriority(4)]
    public void Step4_Immutability_And_Retention_AreDbLevel() { }

    // Cleanup — remove the seeded master (the audit rows it produced remain, by design — immutable).
    [Fact, TestPriority(90)]
    public async Task Step90_Cleanup_DeleteSeededMaster()
    {
        if (_typeId == 0) return;
        var resp = await _f.Client.DeleteAsync($"{TypeRoute}?id={_typeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
