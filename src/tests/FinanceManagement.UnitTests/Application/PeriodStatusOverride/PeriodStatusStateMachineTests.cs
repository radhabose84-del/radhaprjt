using FinanceManagement.Application.Common.PeriodStatus;

namespace FinanceManagement.UnitTests.Application.PeriodStatusOverride
{
    /// <summary>
    /// US-GL03-02 — state machine guards. The single security-critical helper.
    /// Forward: OPEN → SOFTCLOSED → HARDCLOSED. Reverse: HARDCLOSED → SOFTCLOSED, SOFTCLOSED → OPEN.
    /// </summary>
    public sealed class PeriodStatusStateMachineTests
    {
        // ─── AssertForwardTransition — happy paths ──────────────────────────

        [Theory]
        [InlineData("OPEN",       "SOFTCLOSED")]
        [InlineData("SOFTCLOSED", "HARDCLOSED")]
        public void AssertForwardTransition_ValidPair_DoesNotThrow(string from, string to)
        {
            var act = () => PeriodStatusStateMachine.AssertForwardTransition(from, to);
            act.Should().NotThrow();
        }

        [Theory]
        [InlineData("open",       "softclosed")]   // lower-case
        [InlineData("Open",       "SoftClosed")]    // mixed-case
        public void AssertForwardTransition_CaseInsensitive_DoesNotThrow(string from, string to)
        {
            var act = () => PeriodStatusStateMachine.AssertForwardTransition(from, to);
            act.Should().NotThrow();
        }

        // ─── AssertForwardTransition — illegal pairs ────────────────────────

        [Theory]
        [InlineData("OPEN",       "HARDCLOSED")]   // skip step
        [InlineData("HARDCLOSED", "OPEN")]         // reversal
        [InlineData("HARDCLOSED", "SOFTCLOSED")]   // reversal
        [InlineData("SOFTCLOSED", "OPEN")]         // reversal
        [InlineData("OPEN",       "OPEN")]         // no-op
        [InlineData("SOFTCLOSED", "SOFTCLOSED")]   // no-op
        public void AssertForwardTransition_IllegalPair_Throws(string from, string to)
        {
            var act = () => PeriodStatusStateMachine.AssertForwardTransition(from, to);
            act.Should().Throw<ExceptionRules>()
               .WithMessage("*Illegal status transition*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AssertForwardTransition_NullOrEmptyFrom_Throws(string? from)
        {
            var act = () => PeriodStatusStateMachine.AssertForwardTransition(from, "SOFTCLOSED");
            act.Should().Throw<ExceptionRules>()
               .WithMessage("*unknown*");
        }

        // ─── IsValidReversal ────────────────────────────────────────────────

        [Theory]
        [InlineData("HARDCLOSED", "SOFTCLOSED", true)]
        [InlineData("SOFTCLOSED", "OPEN",       true)]
        [InlineData("OPEN",       "SOFTCLOSED", false)]   // forward, not reversal
        [InlineData("SOFTCLOSED", "HARDCLOSED", false)]   // forward, not reversal
        [InlineData("HARDCLOSED", "OPEN",       false)]   // must go via SOFTCLOSED
        [InlineData("OPEN",       "OPEN",       false)]
        public void IsValidReversal_ReturnsExpected(string from, string to, bool expected)
        {
            PeriodStatusStateMachine.IsValidReversal(from, to).Should().Be(expected);
        }

        [Theory]
        [InlineData(null,   "OPEN")]
        [InlineData("HARDCLOSED", null)]
        [InlineData(null,   null)]
        [InlineData("",     "OPEN")]
        public void IsValidReversal_NullOrEmpty_ReturnsFalse(string? from, string? to)
        {
            PeriodStatusStateMachine.IsValidReversal(from, to).Should().BeFalse();
        }

        [Theory]
        [InlineData("hardclosed", "softclosed")]
        [InlineData("HARDCLOSED", "softclosed")]
        public void IsValidReversal_CaseInsensitive_ReturnsTrue(string from, string to)
        {
            PeriodStatusStateMachine.IsValidReversal(from, to).Should().BeTrue();
        }
    }
}
