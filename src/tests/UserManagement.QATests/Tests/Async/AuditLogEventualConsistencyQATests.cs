namespace UserManagement.QATests.Tests.Async;

// ─────────────────────────────────────────────────────────────────────────────
// Phase 4 — Async / eventual-consistency.
//
// The QA/Functional suites assert SYNCHRONOUS API responses. This slice asserts a
// DOWNSTREAM side-effect: a write action publishes AuditLogsDomainEvent, which is
// persisted to the shared MongoDB `AuditLogs` collection and becomes queryable via
// a DIFFERENT module's read endpoint (BudgetManagement audit search). It uses the
// act → POLL → assert pattern so any propagation lag is tolerated.
//
// Runs with only BSOFT.Api hosting the request pipeline + MongoDB. The full
// RabbitMQ → MassTransit consumer → in-app/SignalR notification path is a separate
// concern that needs BSOFT.Worker + RabbitMQ running and is NOT covered here.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("AsyncCollection")]
[Trait("Layer", "Async")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class AuditLogEventualConsistencyQATests
{
    private readonly QAServerFixture _f;

    private const string DivisionRoute    = "/api/Division";
    private const string AuditSearchRoute = "/api/budget/AuditLog/GetAuditLogSearch";
    private const int    QACompanyId      = 1;

    public AuditLogEventualConsistencyQATests(QAServerFixture fixture) => _f = fixture;

    [Fact, TestPriority(1)]
    public async Task Create_PublishesAuditLog_ObservableViaAuditSearch()
    {
        // Run-unique token embedded in the Division name. The Division create handler writes
        // details: "Division '{Name}' was created..." → the token lands in the audit Details,
        // which the BudgetManagement audit search matches (regex over UserName/Action/Details).
        var token = _f.EntityCode;

        // ACT — synchronous create returns 200…
        var createResp = await _f.Client.PostAsJsonAsync(DivisionRoute, new
        {
            shortName = _f.EntityCode[..6],
            name      = $"QA Async {token}",
            companyId = QACompanyId
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // ASSERT (eventual) — …and the audit record becomes observable downstream.
        var observed = await PollUntilAsync(async () =>
        {
            var resp = await _f.Client.GetAsync(
                $"{AuditSearchRoute}?searchPattern={Uri.EscapeDataString(token)}");
            if (resp.StatusCode != HttpStatusCode.OK) return false;
            var body = await resp.Content.ReadAsStringAsync();
            return body.Contains(token, StringComparison.OrdinalIgnoreCase);
        }, attempts: 15, delayMs: 1000);

        observed.Should().BeTrue(
            $"the create audit for '{token}' must be persisted to MongoDB and returned by the " +
            "audit search within the poll window (event → MongoDB → cross-module read).");
    }

    [Fact, TestPriority(2)]
    public async Task AuditSearch_UnknownPattern_ReturnsNoMatch()
    {
        // Control: a token that was never written must NOT be found — proves the positive
        // result above is a real match, not the endpoint echoing everything.
        var resp = await _f.Client.GetAsync(
            $"{AuditSearchRoute}?searchPattern=ZZZNOAUDIT{_f.EntityCode}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadAsStringAsync();
        body.Should().NotContain($"ZZZNOAUDIT{_f.EntityCode}");
    }

    private static async Task<bool> PollUntilAsync(Func<Task<bool>> condition, int attempts, int delayMs)
    {
        for (var i = 0; i < attempts; i++)
        {
            if (await condition()) return true;
            await Task.Delay(delayMs);
        }
        return false;
    }
}
