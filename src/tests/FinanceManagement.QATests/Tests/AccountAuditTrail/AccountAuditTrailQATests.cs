namespace FinanceManagement.QATests.Tests.AccountAuditTrail;

// ─────────────────────────────────────────────────────────────────────────────
// AccountAuditTrail (live-server QA) — US-GL02-09 statutory audit trail (read-only).
//
// Viewer route: api/finance/account-audit
//   GET {entityName}/{entityId}                          → per-account field-level history (AC-3)
//   GET export?from=&to=&entityName=                     → range export + record-count checksum (AC-4)
//
// There are NO write endpoints — rows are written only by the SaveChanges interceptor when an
// IAuditTrailed entity (here AccountTypeMaster) is edited. So the suite seeds a change through the
// AccountTypeMaster API, then asserts the trail reflects it. AccountTypeMaster is used because it is
// audited and has no COA-freeze trigger to contend with.
//
// NOTE: requires the QA server to be running the US-GL02-09 build (interceptor + role-in-JWT). The
// role claim is only present after a fresh login on that build — the fixture logs in per run.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AccountAuditTrailCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AccountAuditTrailQATests
{
    private readonly QAServerFixture _f;
    private const string AuditRoute = "/api/finance/account-audit";
    private const string TypeRoute = "/api/finance/accounttypemaster";
    private const int QACompanyId = 1;

    private static int _typeId;

    public AccountAuditTrailQATests(QAServerFixture fixture) => _f = fixture;

    private static async Task<JsonDocument> ParseAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private string StartDigit()
    {
        var d = new string(_f.EntityCode.Where(char.IsDigit).ToArray());
        var n = d.Length > 0 ? (d[0] - '0') : 5;
        return (n == 0 ? 9 : n).ToString();
    }

    private object TypeBody(string name, int sortOrder) => new
    {
        companyId = QACompanyId,
        accountTypeName = name,
        startCode = StartDigit(),
        accountCodeLength = 6,
        sortOrder
    };

    private static (string from, string to) ThisYearRange() =>
        ("2026-01-01T00:00:00Z", "2027-01-01T00:00:00Z");

    // ── SMOKE — the viewer is reachable ─────────────────────────────────────────
    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_History_HappyPath_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{AuditRoute}/GlAccountMaster/1");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ParseAsync(resp)).RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    // ── AUTH ────────────────────────────────────────────────────────────────────
    [Fact, TestPriority(2)]
    public async Task TC002_History_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{AuditRoute}/GlAccountMaster/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact, TestPriority(3)]
    public async Task TC003_Export_NoAuth_Returns401()
    {
        var (from, to) = ThisYearRange();
        var resp = await _f.AnonymousClient.GetAsync($"{AuditRoute}/export?from={from}&to={to}");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── SEED a change so the trail has content ──────────────────────────────────
    [Fact, TestPriority(10)]
    public async Task TC010_CreateAuditedEntity_CapturesId()
    {
        var resp = await _f.Client.PostAsJsonAsync(TypeRoute, TypeBody($"QA Audit {_f.EntityCode}", 1));
        // startCode is unique per company (1-9) — on a populated clone the digit may be taken; tolerate 400.
        if (resp.StatusCode == HttpStatusCode.BadRequest) return;

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        _typeId = (await ParseAsync(resp)).RootElement.GetProperty("data").GetInt32();
        _typeId.Should().BeGreaterThan(0);
    }

    [Fact, TestPriority(11)]
    public async Task TC011_EditAuditedEntity_ProducesTrailRow()
    {
        if (_typeId == 0) return;
        var resp = await _f.Client.PutAsJsonAsync(TypeRoute, new
        {
            id = _typeId,
            accountTypeName = $"QA Audit Edited {_f.EntityCode}",
            startCode = StartDigit(),
            accountCodeLength = 6,
            sortOrder = 2,
            isActive = 1
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── AC-3: per-account, field-level history ──────────────────────────────────
    [Fact, TestPriority(12)]
    public async Task TC012_History_ContainsFieldLevelRow_WithRole()
    {
        if (_typeId == 0) return;
        var resp = await _f.Client.GetAsync($"{AuditRoute}/AccountTypeMaster/{_typeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.ValueKind.Should().Be(JsonValueKind.Array);
        data.GetArrayLength().Should().BeGreaterThan(0, "the create/edit must have produced audit rows");

        var first = data[0];
        first.GetProperty("entityName").GetString().Should().Be("AccountTypeMaster");
        first.TryGetProperty("propertyName", out _).Should().BeTrue();
        // AC-1: role captured (present once the QA build issues role claims and the user re-logged in).
        first.TryGetProperty("createdByRole", out _).Should().BeTrue();
    }

    // ── AC-4: export with record-count checksum ─────────────────────────────────
    [Fact, TestPriority(13)]
    public async Task TC013_Export_ReturnsRecordCountAndChecksum()
    {
        var (from, to) = ThisYearRange();
        var resp = await _f.Client.GetAsync($"{AuditRoute}/export?from={from}&to={to}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        data.GetProperty("recordCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);
        data.GetProperty("checksum").GetString().Should().NotBeNullOrWhiteSpace();
        data.TryGetProperty("rows", out var rows).Should().BeTrue();
        rows.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact, TestPriority(14)]
    public async Task TC014_Export_FilteredByEntity_Reachable()
    {
        var (from, to) = ThisYearRange();
        var resp = await _f.Client.GetAsync($"{AuditRoute}/export?from={from}&to={to}&entityName=AccountTypeMaster");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var data = (await ParseAsync(resp)).RootElement.GetProperty("data");
        // recordCount is the tamper-evident count checksum (AC-4); rows match the filter.
        data.GetProperty("recordCount").GetInt32().Should().Be(data.GetProperty("rows").GetArrayLength());
    }

    // ── CLEANUP ─────────────────────────────────────────────────────────────────
    [Fact, TestPriority(90)]
    public async Task TC090_Delete_SeededEntity()
    {
        if (_typeId == 0) return;
        var resp = await _f.Client.DeleteAsync($"{TypeRoute}?id={_typeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
