using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IBackdateEnforcement;
using FinanceManagement.Infrastructure.Services;

namespace FinanceManagement.UnitTests.Infrastructure.Services
{
    /// <summary>
    /// US-GL03-04 — the decision matrix for backdated postings. Critical: this is what the future
    /// posting middleware calls on every JE. Covers all 5 branches:
    ///   1) VoucherDate &gt;= today → not backdated
    ///   2) VoucherDate &lt;  today + period OPEN
    ///   3) VoucherDate &lt;  today + period SOFTCLOSED (reason required)
    ///   4) VoucherDate &lt;  today + period HARDCLOSED (still flagged backdated)
    ///   5) VoucherDate &lt;  today + period not found
    /// </summary>
    public sealed class BackdateEnforcementServiceTests
    {
        private readonly Mock<IFinancialPeriodMasterLookup> _mockPeriodLookup = new(MockBehavior.Strict);

        private BackdateEnforcementService CreateSut() => new(_mockPeriodLookup.Object);

        private void SetupPeriod(string? statusCode)
        {
            _mockPeriodLookup
                .Setup(r => r.GetPeriodForDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(statusCode == null
                    ? null
                    : new FinancialPeriodMasterLookupDto { StatusCode = statusCode });
        }

        // ─── Branch 1: forward or same-day ──────────────────────────────────

        [Fact]
        public async Task Evaluate_FutureVoucherDate_NotBackdated_NoReasonNeeded()
        {
            var today = new DateOnly(2026, 6, 25);
            var future = today.AddDays(7);

            var decision = await CreateSut().EvaluateAsync(1, future, today, null, CancellationToken.None);

            decision.IsBackdated.Should().BeFalse();
            decision.RequiresReason.Should().BeFalse();
            decision.RejectMessage.Should().BeNull();
            _mockPeriodLookup.VerifyNoOtherCalls(); // never even queried
        }

        [Fact]
        public async Task Evaluate_SameDayVoucherDate_NotBackdated()
        {
            var today = new DateOnly(2026, 6, 25);

            var decision = await CreateSut().EvaluateAsync(1, today, today, null, CancellationToken.None);

            decision.IsBackdated.Should().BeFalse();
            decision.RequiresReason.Should().BeFalse();
        }

        // ─── Branch 2: backdated into OPEN period ───────────────────────────

        [Fact]
        public async Task Evaluate_BackdatedIntoOpenPeriod_FlagsOnly_NoReason()
        {
            SetupPeriod("OPEN");
            var today = new DateOnly(2026, 6, 25);
            var voucherDate = today.AddDays(-5);

            var decision = await CreateSut().EvaluateAsync(1, voucherDate, today, null, CancellationToken.None);

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeFalse();
            decision.RejectMessage.Should().BeNull();
        }

        // ─── Branch 3: backdated into SOFTCLOSED ────────────────────────────

        [Fact]
        public async Task Evaluate_BackdatedIntoSoftClosed_NoReason_Rejected()
        {
            SetupPeriod("SOFTCLOSED");
            var today = new DateOnly(2026, 6, 25);
            var voucherDate = today.AddDays(-30);

            var decision = await CreateSut().EvaluateAsync(1, voucherDate, today, backdateReason: null, CancellationToken.None);

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeTrue();
            decision.ReasonMissing.Should().BeTrue();
            decision.RejectMessage.Should().NotBeNullOrEmpty();
            decision.RejectMessage.Should().Contain("reason");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Evaluate_BackdatedIntoSoftClosed_EmptyOrWhitespaceReason_Rejected(string? reason)
        {
            SetupPeriod("SOFTCLOSED");
            var today = new DateOnly(2026, 6, 25);

            var decision = await CreateSut().EvaluateAsync(
                1, today.AddDays(-7), today, reason, CancellationToken.None);

            decision.ReasonMissing.Should().BeTrue();
            decision.RejectMessage.Should().NotBeNull();
        }

        [Fact]
        public async Task Evaluate_BackdatedIntoSoftClosed_WithReason_Allowed()
        {
            SetupPeriod("SOFTCLOSED");
            var today = new DateOnly(2026, 6, 25);

            var decision = await CreateSut().EvaluateAsync(
                1, today.AddDays(-7), today, "Bank charge accrual from prior month", CancellationToken.None);

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeTrue();
            decision.ReasonMissing.Should().BeFalse();
            decision.RejectMessage.Should().BeNull();
        }

        [Fact]
        public async Task Evaluate_BackdatedIntoSoftClosed_LowercaseStatus_StillTreatedAsSoftClosed()
        {
            // Defence: lookup returns 'softclosed' instead of canonical 'SOFTCLOSED' → still rejected.
            SetupPeriod("softclosed");
            var today = new DateOnly(2026, 6, 25);

            var decision = await CreateSut().EvaluateAsync(
                1, today.AddDays(-1), today, null, CancellationToken.None);

            decision.ReasonMissing.Should().BeTrue();
        }

        // ─── Branch 4: backdated into HARDCLOSED ────────────────────────────

        [Fact]
        public async Task Evaluate_BackdatedIntoHardClosed_FlagsBackdated_NoReasonRequired()
        {
            // Service does NOT reject HardClosed — IPeriodPostingGate owns that. We just flag the entry
            // as backdated and let the gate block the post.
            SetupPeriod("HARDCLOSED");
            var today = new DateOnly(2026, 6, 25);

            var decision = await CreateSut().EvaluateAsync(
                1, today.AddDays(-90), today, null, CancellationToken.None);

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeFalse();
            decision.RejectMessage.Should().BeNull();
        }

        // ─── Branch 5: period not found ─────────────────────────────────────

        [Fact]
        public async Task Evaluate_BackdatedNoPeriodFound_FlagsBackdated_NoReasonRequired()
        {
            // Service does NOT reject — the posting handler's open-period resolver does.
            SetupPeriod(statusCode: null);
            var today = new DateOnly(2026, 6, 25);

            var decision = await CreateSut().EvaluateAsync(
                1, today.AddDays(-2), today, null, CancellationToken.None);

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeFalse();
            decision.RejectMessage.Should().BeNull();
        }

        // ─── Static factories self-check ────────────────────────────────────

        [Fact]
        public void BackdateDecision_Allowed_SetsCorrectFlags()
        {
            var decision = BackdateDecision.Allowed(isBackdated: true, requiresReason: true);

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeTrue();
            decision.ReasonMissing.Should().BeFalse();
            decision.RejectMessage.Should().BeNull();
        }

        [Fact]
        public void BackdateDecision_ReasonRequired_HasRejectMessage()
        {
            var decision = BackdateDecision.ReasonRequired();

            decision.IsBackdated.Should().BeTrue();
            decision.RequiresReason.Should().BeTrue();
            decision.ReasonMissing.Should().BeTrue();
            decision.RejectMessage.Should().NotBeNullOrEmpty();
        }
    }
}
