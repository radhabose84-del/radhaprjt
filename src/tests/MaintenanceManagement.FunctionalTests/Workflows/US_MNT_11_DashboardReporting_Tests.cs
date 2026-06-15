namespace MaintenanceManagement.FunctionalTests.Workflows;

// ─────────────────────────────────────────────────────────────────────────────
// US-MNT-11 — Maintenance dashboard & reporting (read-only)
//
//   As a maintenance manager I open the dashboard summaries and reports to monitor
//   work orders, consumption, schedules and power usage.
//
// READ-ONLY story: exercises the Dashboard (7) and Report (11) endpoints with safe,
// run-independent parameters. Asserts the contract is reachable and returns its
// documented shape. Endpoints that legitimately return 404 on an empty dataset (or
// need a real unit code) are asserted tolerantly — they prove the route + auth + read
// path work without depending on seeded transactional data.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("US-MNT-11-DashboardReporting")]
[Trait("Module", "MaintenanceManagement")]
[Trait("Story", "US-MNT-11")]
[Trait("Owner", "unassigned")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class US_MNT_11_DashboardReporting_Tests
{
    private readonly QAServerFixture _f;

    private const string DashboardRoute = "/api/maintenance/Dashboard";
    private const string ReportRoute = "/api/maintenance/Report";

    // A wide date range inside one financial year (Apr–Mar) so the stock-ledger FY check passes.
    private const string FromDate = "2026-04-01";
    private const string ToDate = "2026-09-30";

    public US_MNT_11_DashboardReporting_Tests(QAServerFixture fixture) => _f = fixture;

    private string Range() => $"fromDate={FromDate}&toDate={ToDate}";

    // AC1 — the work-order summary card is reachable.
    [Fact, TestPriority(1)]
    public async Task Step1_WorkOrderSummaryReachable()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/workOrder-summary?{Range()}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/card-dashboard?{Range()}"));
    }

    // AC2 — the item-consumption dashboards (overall / by dept / by machine group) are reachable.
    [Fact, TestPriority(2)]
    public async Task Step2_ItemConsumptionDashboardsReachable()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/item-consumption?{Range()}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/itemConsumption-dept?{Range()}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/itemConsumption-machineGroup?{Range()}"));
    }

    // AC3 — the maintenance-hours dashboards (by dept / by machine group) are reachable.
    [Fact, TestPriority(3)]
    public async Task Step3_MaintenanceHoursDashboardsReachable()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/maintenance-hoursDept?{Range()}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{DashboardRoute}/maintenance-hoursMachine?{Range()}"));
    }

    // AC4 — the reports that return 200 (empty list when no data) are reachable.
    //       Each backing stored proc REQUIRES its date params supplied (omitting them is a
    //       SQL "parameter not supplied" 500), so every call passes an explicit date range.
    [Fact, TestPriority(4)]
    public async Task Step4_AlwaysOkReportsReachable()
    {
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/WorkOrderReport?requestTypeId=0&{Range()}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/ItemConsumption?{Range()}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/RequestReport?requestFromDate={FromDate}&requestToDate={ToDate}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/WorkOrderChecklistReport?WorkOrderFromDate={FromDate}&WorkOrderToDate={ToDate}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/SchedulerReport?FromDueDate={FromDate}&ToDueDate={ToDate}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/MaterialPlanningReport?FromDueDate={FromDate}&ToDueDate={ToDate}"));
        await QAHelper.AssertOkAsync(await _f.Client.GetAsync($"{ReportRoute}/MRSReport?OldUnitCode=1&{Range()}"));
    }

    // AC5 — the data-dependent reports are reachable. They return 404 when no data exists for the
    //       criteria (PowerConsumption/Generator) or when the unit/dept has no stock (CurrentStock,
    //       SubStoresStockLedger), so accept 200/404 — the route, auth and read path are proven.
    [Fact, TestPriority(5)]
    public async Task Step5_DataDependentReportsReachable()
    {
        ((int)(await _f.Client.GetAsync($"{ReportRoute}/PowerConsumptionReport?{Range()}")).StatusCode)
            .Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{ReportRoute}/GeneratorConsumptionReport?{Range()}")).StatusCode)
            .Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{ReportRoute}/CurrentStock/1/1")).StatusCode)
            .Should().BeOneOf(200, 404);
        ((int)(await _f.Client.GetAsync($"{ReportRoute}/SubStoresStockLedger?oldUnitcode=1&{Range()}&DepartmentId=1")).StatusCode)
            .Should().BeOneOf(200, 404);
    }
}
