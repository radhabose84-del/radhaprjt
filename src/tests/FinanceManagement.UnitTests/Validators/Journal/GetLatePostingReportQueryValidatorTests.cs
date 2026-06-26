using FinanceManagement.Application.JournalMaster.Journal.Queries.GetLatePostingReport;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    /// <summary>
    /// US-GL03-04 / AC#3 — the validator is the SQL-injection defence for the report's ORDER BY
    /// concatenation. Tests every accepted SortBy/SortDirection + rejection of arbitrary strings.
    /// </summary>
    public sealed class GetLatePostingReportQueryValidatorTests
    {
        private GetLatePostingReportQueryValidator CreateValidator() => new();

        private static GetLatePostingReportQuery Valid(
            int page = 1, int size = 50,
            string? sortBy = "PostedAt", string? sortDirection = "DESC") =>
            new()
            {
                PageNumber = page,
                PageSize = size,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

        // ─── Happy path ─────────────────────────────────────────────────────

        [Fact]
        public async Task Validate_Defaults_Passes()
        {
            var result = await CreateValidator().TestValidateAsync(Valid());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NoSortFields_Passes()
        {
            var result = await CreateValidator().TestValidateAsync(
                Valid(sortBy: null, sortDirection: null));
            result.ShouldNotHaveAnyValidationErrors();
        }

        // ─── Pagination ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_NonPositivePageNumber_Fails(int page)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(page: page));
            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(201)]
        [InlineData(-1)]
        public async Task Validate_OutOfRangePageSize_Fails(int size)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(size: size));
            result.ShouldHaveValidationErrorFor(x => x.PageSize);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        [InlineData(200)]
        public async Task Validate_BoundaryPageSize_Passes(int size)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(size: size));
            result.ShouldNotHaveValidationErrorFor(x => x.PageSize);
        }

        // ─── AccountingPeriodId ─────────────────────────────────────────────

        [Fact]
        public async Task Validate_NullAccountingPeriodId_Passes()
        {
            var cmd = Valid();
            cmd.AccountingPeriodId = null;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveValidationErrorFor(x => x.AccountingPeriodId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_NonPositiveAccountingPeriodId_Fails(int id)
        {
            var cmd = Valid();
            cmd.AccountingPeriodId = id;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.AccountingPeriodId);
        }

        // ─── Date range ─────────────────────────────────────────────────────

        [Fact]
        public async Task Validate_FromAfterTo_Fails()
        {
            var cmd = Valid();
            cmd.FromDate = new DateOnly(2026, 6, 30);
            cmd.ToDate   = new DateOnly(2026, 6, 1);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public async Task Validate_FromBeforeTo_Passes()
        {
            var cmd = Valid();
            cmd.FromDate = new DateOnly(2026, 6, 1);
            cmd.ToDate   = new DateOnly(2026, 6, 30);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_OnlyFromDate_Passes()
        {
            var cmd = Valid();
            cmd.FromDate = new DateOnly(2026, 6, 1);

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }

        // ─── SortBy allow-list ──────────────────────────────────────────────

        [Theory]
        [InlineData("PostedAt")]
        [InlineData("VoucherDate")]
        [InlineData("DaysBackdated")]
        [InlineData("postedat")]  // case-insensitive
        public async Task Validate_AllowedSortBy_Passes(string sortBy)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(sortBy: sortBy));
            result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
        }

        [Theory]
        [InlineData("Id")]
        [InlineData("Random")]
        [InlineData("PostedAt; DROP TABLE x; --")]
        [InlineData("h.Id")]
        public async Task Validate_NonAllowedSortBy_Fails(string sortBy)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(sortBy: sortBy));
            result.ShouldHaveValidationErrorFor(x => x.SortBy);
        }

        // ─── SortDirection allow-list ───────────────────────────────────────

        [Theory]
        [InlineData("ASC")]
        [InlineData("DESC")]
        [InlineData("asc")]
        [InlineData("desc")]
        public async Task Validate_AllowedSortDirection_Passes(string dir)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(sortDirection: dir));
            result.ShouldNotHaveValidationErrorFor(x => x.SortDirection);
        }

        [Theory]
        [InlineData("FOO")]
        [InlineData("UP")]
        [InlineData("DESC; --")]
        public async Task Validate_NonAllowedSortDirection_Fails(string dir)
        {
            var result = await CreateValidator().TestValidateAsync(Valid(sortDirection: dir));
            result.ShouldHaveValidationErrorFor(x => x.SortDirection);
        }
    }
}
