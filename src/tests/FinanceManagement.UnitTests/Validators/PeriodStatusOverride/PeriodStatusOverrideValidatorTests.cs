using FluentValidation.TestHelper;
using FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed;
using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToSoftClosed;
using FinanceManagement.Presentation.Validation.PeriodStatusOverride;

namespace FinanceManagement.UnitTests.Validators.PeriodStatusOverride
{
    /// <summary>
    /// All 5 US-GL03-02 validators consolidated — each is a thin payload guard.
    /// Business rules (state machine, role check) live in handlers.
    /// </summary>
    public sealed class PeriodStatusOverrideValidatorTests
    {
        // ─── TransitionPeriodToSoftClosed ──────────────────────────────────

        [Fact]
        public async Task SoftClose_ZeroPeriodId_Fails()
        {
            var v = new TransitionPeriodToSoftClosedCommandValidator();
            var result = await v.TestValidateAsync(new TransitionPeriodToSoftClosedCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.PeriodId);
        }

        [Fact]
        public async Task SoftClose_ValidPeriodId_Passes()
        {
            var v = new TransitionPeriodToSoftClosedCommandValidator();
            var result = await v.TestValidateAsync(new TransitionPeriodToSoftClosedCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        // ─── TransitionPeriodToHardClosed ──────────────────────────────────

        [Fact]
        public async Task HardClose_ZeroPeriodId_Fails()
        {
            var v = new TransitionPeriodToHardClosedCommandValidator();
            var result = await v.TestValidateAsync(new TransitionPeriodToHardClosedCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.PeriodId);
        }

        // ─── RequestPeriodReversal ─────────────────────────────────────────

        [Fact]
        public async Task Request_ValidCommand_Passes()
        {
            var v = new RequestPeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new RequestPeriodReversalCommand
            {
                PeriodId = 1, TargetStatusCode = "SOFTCLOSED", RequestedReason = "Audit"
            });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Request_EmptyReason_Fails(string? reason)
        {
            var v = new RequestPeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new RequestPeriodReversalCommand
            {
                PeriodId = 1, TargetStatusCode = "SOFTCLOSED", RequestedReason = reason
            });
            result.ShouldHaveValidationErrorFor(x => x.RequestedReason);
        }

        [Theory]
        [InlineData("HARDCLOSED")]      // hard-close not a valid target (forward only)
        [InlineData("PENDING")]         // not a period status
        [InlineData("UNKNOWN")]
        public async Task Request_InvalidTargetCode_Fails(string target)
        {
            var v = new RequestPeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new RequestPeriodReversalCommand
            {
                PeriodId = 1, TargetStatusCode = target, RequestedReason = "x"
            });
            result.ShouldHaveValidationErrorFor(x => x.TargetStatusCode);
        }

        [Fact]
        public async Task Request_ReasonTooLong_Fails()
        {
            var v = new RequestPeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new RequestPeriodReversalCommand
            {
                PeriodId = 1, TargetStatusCode = "SOFTCLOSED",
                RequestedReason = new string('x', 501)
            });
            result.ShouldHaveValidationErrorFor(x => x.RequestedReason);
        }

        // ─── ApprovePeriodReversal ─────────────────────────────────────────

        [Theory]
        [InlineData("CFO")]
        [InlineData("SysAdmin")]
        [InlineData("cfo")]            // case-insensitive
        [InlineData("sysadmin")]
        public async Task Approve_ValidRole_Passes(string role)
        {
            var v = new ApprovePeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new ApprovePeriodReversalCommand
            {
                OverrideId = 1, Role = role
            });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("FinanceManager")]
        [InlineData("admin")]
        [InlineData("Anyone")]
        public async Task Approve_InvalidRole_Fails(string role)
        {
            var v = new ApprovePeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new ApprovePeriodReversalCommand
            {
                OverrideId = 1, Role = role
            });
            result.ShouldHaveValidationErrorFor(x => x.Role);
        }

        [Fact]
        public async Task Approve_EmptyRole_Fails()
        {
            var v = new ApprovePeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new ApprovePeriodReversalCommand
            {
                OverrideId = 1, Role = ""
            });
            result.ShouldHaveValidationErrorFor(x => x.Role);
        }

        // ─── RejectPeriodReversal ──────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Reject_EmptyReason_Fails(string? reason)
        {
            var v = new RejectPeriodReversalCommandValidator();
            var result = await v.TestValidateAsync(new RejectPeriodReversalCommand
            {
                OverrideId = 1, RejectionReason = reason
            });
            result.ShouldHaveValidationErrorFor(x => x.RejectionReason);
        }
    }
}
