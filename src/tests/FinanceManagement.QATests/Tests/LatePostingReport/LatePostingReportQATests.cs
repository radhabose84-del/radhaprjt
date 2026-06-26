using static Shared.QAInfrastructure.Helpers.QAHelper;

namespace FinanceManagement.QATests.Tests.LatePostingReport;

// ─────────────────────────────────────────────────────────────────────────────
// US-GL03-04 — Late-posting report endpoint (/api/finance/Journal/late-posting-report).
// QA scope: prove the controller contract — auth, validation, pagination, sort
// allow-list, date-range filter, and smoke-tag the happy path so CI deploy gate
// catches a broken read path.
//
// Notes for live reconciliation:
//   * The QA clone may or may not have actual backdated postings. Smoke just asserts
//     a 200 with a (possibly empty) data array — that is what matters as a deploy gate.
//   * Sort allow-list rejects with 400 — this is the SQLi defence and MUST stay green.
// ─────────────────────────────────────────────────────────────────────────────

[Collection("LatePostingReportCollection")]
[TestCaseOrderer("Shared.QAInfrastructure.Infrastructure.PriorityOrderer", "Shared.QAInfrastructure")]
public sealed class LatePostingReportQATests
{
    private readonly QAServerFixture _f;
    private const string Route = "/api/finance/Journal/late-posting-report";

    public LatePostingReportQATests(QAServerFixture fixture) => _f = fixture;

    // ─── SMOKE ──────────────────────────────────────────────────────────────

    [Fact, TestPriority(1)]
    [Trait("Layer", "Smoke")]
    public async Task TC001_GetLatePostingReport_DefaultPaging_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=50");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var data = doc.RootElement.GetProperty("data");
        data.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Null);
    }

    // ─── AUTH ──────────────────────────────────────────────────────────────

    [Fact, TestPriority(2)]
    public async Task TC002_GetLatePostingReport_NoAuth_Returns401()
    {
        var resp = await _f.AnonymousClient.GetAsync($"{Route}?PageNumber=1&PageSize=50");
        await Assert401Async(resp);
    }

    // ─── PAGINATION VALIDATION ─────────────────────────────────────────────

    [Theory, TestPriority(10)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task TC010_GetLatePostingReport_NonPositivePageNumber_Returns400(int page)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber={page}&PageSize=50");
        await Assert400Async(resp);
    }

    [Theory, TestPriority(11)]
    [InlineData(0)]
    [InlineData(201)]
    public async Task TC011_GetLatePostingReport_OutOfRangePageSize_Returns400(int size)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize={size}");
        await Assert400Async(resp);
    }

    [Theory, TestPriority(12)]
    [InlineData(1)]
    [InlineData(200)]
    public async Task TC012_GetLatePostingReport_BoundaryPageSize_Returns200(int size)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize={size}");
        await AssertOkAsync(resp);
    }

    // ─── SORT ALLOW-LIST (SQLi defence) ────────────────────────────────────

    [Theory, TestPriority(20)]
    [InlineData("PostedAt")]
    [InlineData("VoucherDate")]
    [InlineData("DaysBackdated")]
    public async Task TC020_GetLatePostingReport_AllowedSortBy_Returns200(string sortBy)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&SortBy={sortBy}&SortDirection=DESC");
        await AssertOkAsync(resp);
    }

    [Theory, TestPriority(21)]
    [InlineData("Id")]
    [InlineData("Random")]
    [InlineData("h.Id")]
    public async Task TC021_GetLatePostingReport_NonAllowedSortBy_Returns400(string sortBy)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&SortBy={sortBy}");
        await Assert400Async(resp);
    }

    [Theory, TestPriority(22)]
    [InlineData("ASC")]
    [InlineData("DESC")]
    public async Task TC022_GetLatePostingReport_AllowedSortDirection_Returns200(string dir)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&SortBy=PostedAt&SortDirection={dir}");
        await AssertOkAsync(resp);
    }

    [Theory, TestPriority(23)]
    [InlineData("FOO")]
    [InlineData("UP")]
    public async Task TC023_GetLatePostingReport_NonAllowedSortDirection_Returns400(string dir)
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&SortDirection={dir}");
        await Assert400Async(resp);
    }

    // ─── DATE RANGE ────────────────────────────────────────────────────────

    [Fact, TestPriority(30)]
    public async Task TC030_GetLatePostingReport_FromAfterTo_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&FromDate=2026-06-30&ToDate=2026-06-01");
        await Assert400Async(resp);
    }

    [Fact, TestPriority(31)]
    public async Task TC031_GetLatePostingReport_FromBeforeTo_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&FromDate=2026-06-01&ToDate=2026-06-30");
        await AssertOkAsync(resp);
    }

    [Fact, TestPriority(32)]
    public async Task TC032_GetLatePostingReport_OnlyFromDate_Returns200()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&FromDate=2026-06-01");
        await AssertOkAsync(resp);
    }

    // ─── ACCOUNTING PERIOD FILTER ──────────────────────────────────────────

    [Fact, TestPriority(40)]
    public async Task TC040_GetLatePostingReport_InvalidAccountingPeriodId_Returns400()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10&AccountingPeriodId=0");
        await Assert400Async(resp);
    }

    // ─── RESPONSE SHAPE ────────────────────────────────────────────────────

    [Fact, TestPriority(50)]
    public async Task TC050_GetLatePostingReport_Response_ContainsExpectedFields()
    {
        var resp = await _f.Client.GetAsync($"{Route}?PageNumber=1&PageSize=10");
        await AssertOkAsync(resp);

        using var doc = await ParseAsync(resp);
        var root = doc.RootElement;

        root.TryGetProperty("isSuccess", out _).Should().BeTrue();
        root.TryGetProperty("data", out _).Should().BeTrue();
        root.TryGetProperty("totalCount", out _).Should().BeTrue();
        root.TryGetProperty("pageNumber", out _).Should().BeTrue();
        root.TryGetProperty("pageSize", out _).Should().BeTrue();
    }
}
